using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dapper;
using Hangfire.Console;
using Hangfire.Server;
using HtmlAgilityPack;
using NReadability;

namespace SyzbWechatBotAPI.Jobs
{
    public class NewsJob
    {
        private readonly IDbConnection _connection;

        public NewsJob(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task Dowload(string url, PerformContext context)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.62 Safari/537.36");
                var response = await client.GetAsync(url);
                if (response.StatusCode != HttpStatusCode.OK)
                    return;

                var stream = await response.Content.ReadAsStreamAsync();
                byte[] bytes = new byte[stream.Length];
                await stream.ReadAsync(bytes, 0, bytes.Length);
                var isUTF8 = IsTextUTF8(ref bytes);
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding encoding;
                if (isUTF8)
                {
                    encoding = Encoding.UTF8;
                }
                else
                {
                    encoding = Encoding.GetEncoding("GBK");
                }

                var html = encoding.GetString(bytes);
                //var document = new HtmlDocument { OptionAutoCloseOnEnd = true };

                //document.LoadHtml(html);
                //foreach (var selectNode in document.DocumentNode.SelectNodes("//meta"))
                //{
                //    if (selectNode.Attributes["http-equiv"]?.Value == "Content-Type")
                //    {
                //        var contentType = selectNode.Attributes["content"].Value;
                //        var match = Regex.Match(contentType, "charset=(?<encoding>[a-zA-Z0-9\\-]*)");
                //        if (match.Success)
                //        {
                //            var encodingName = match.Groups["encoding"].Value;
                //            html = Encoding.GetEncoding(encodingName).GetString(bytes);
                //            break;
                //        }
                //    }

                //    if (selectNode.Attributes["charset"] != null)
                //    {
                //        var encodingName = selectNode.Attributes["charset"].Value;
                //        html = Encoding.GetEncoding(encodingName).GetString(bytes);
                //        break;
                //    }
                //}
                //document.LoadHtml(html);
                //using (var ms = new MemoryStream())
                //using (StreamWriter sw = new StreamWriter(ms, Encoding.UTF8))
                //{
                //    document.Save(sw);
                //    ms.Position = 0;
                //    var xdoc = XDocument.Load(ms);
                //    //using (var sr = new StreamReader(ms))
                //    //{

                //    //    html = await sr.ReadToEndAsync();
                //    //}
                //}

                //var html = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(html))
                    return;

                var transcoder = new NReadabilityTranscoder();
                var input = new TranscodingInput(html);
                try
                {
                    SgmlDomBuilder builder = new SgmlDomBuilder();
                    var s = builder.BuildDocument(html);
                    var result = transcoder.Transcode(input);

                    var document = new HtmlDocument { OptionAutoCloseOnEnd = true };
                    document.LoadHtml(result.ExtractedContent);
                    var node = document.DocumentNode.SelectSingleNode("//div/div/div/div");
                    var text = node.InnerText.Trim('\r', '\n', ' ', '\t');
                    context.WriteLine("抽取内容为:");
                    context.WriteLine(text);

                    const string cmdText = @"UPDATE [dbo].[BaiduNews] SET [Html]=@Html,[Text]=@Text WHERE [Url]=@Url";

                    await _connection.ExecuteAsync(cmdText, new { Html = html, Text = text, Url = url });

                    await _connection.ExecuteAsync(
                        @"UPDATE a SET a.[NewsCount]=a.[NewsCount]+1 FROM [dbo].[Monitor] a JOIN [dbo].[BaiduNews] b ON a.[Tag]=b.[Keyword] WHERE b.[Url]=@Url",
                        new { Url = url });
                }
                catch (Exception e)
                {
                    context.WriteLine(e);
                }
            }
        }

        private bool IsTextUTF8(ref byte[] inputStream)
        {
            int encodingBytesCount = 0;
            bool allTextsAreASCIIChars = true;

            for (int i = 0; i < inputStream.Length; i++)
            {
                byte current = inputStream[i];

                if ((current & 0x80) == 0x80)
                {
                    allTextsAreASCIIChars = false;
                }
                // First byte
                if (encodingBytesCount == 0)
                {
                    if ((current & 0x80) == 0)
                    {
                        // ASCII chars, from 0x00-0x7F
                        continue;
                    }

                    if ((current & 0xC0) == 0xC0)
                    {
                        encodingBytesCount = 1;
                        current <<= 2;

                        // More than two bytes used to encoding a unicode char.
                        // Calculate the real length.
                        while ((current & 0x80) == 0x80)
                        {
                            current <<= 1;
                            encodingBytesCount++;
                        }
                    }
                    else
                    {
                        // Invalid bits structure for UTF8 encoding rule.
                        return false;
                    }
                }
                else
                {
                    // Following bytes, must start with 10.
                    if ((current & 0xC0) == 0x80)
                    {
                        encodingBytesCount--;
                    }
                    else
                    {
                        // Invalid bits structure for UTF8 encoding rule.
                        return false;
                    }
                }
            }

            if (encodingBytesCount != 0)
            {
                // Invalid bits structure for UTF8 encoding rule.
                // Wrong following bytes count.
                return false;
            }

            // Although UTF8 supports encoding for ASCII chars, we regard as a input stream, whose contents are all ASCII as default encoding.
            return !allTextsAreASCIIChars;
        }
    }
}
