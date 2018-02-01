import './newlist.less';
import * as React from 'react';
import fetch from "isomorphic-fetch";
import Media from "react-bootstrap/lib/Media";
import Label from 'react-bootstrap/lib/Label';
import { Link } from 'react-router-dom'

export default class NewsList extends React.Component {
    constructor() {
        super();
        this.state = { newsList: [], loading: true };

    }

    componentDidMount() {
        fetch(`/api/News/Tag/${this.props.tag}`)
            .then(response => response.json())
            .then(json => {
                this.setState({
                    newsList: json, loading: false
                });
            });
    }

    render() {
        const { newsList, loading } = this.state;
        return (
            <Media>
                <Media.Body>
                    <div className="tag">"{this.props.tag}"相关新闻</div>
                    {loading && <div>正中搜索中...</div>}
                    <Media.List>
                        {newsList.map((news, index) => (
                            <Media.ListItem key={index}>
                                <Link to={`/news/${news.id}`}>
                                    <Media.Body>
                                        <Media.Heading>{news.title}</Media.Heading>
                                        <p>{news.summary}</p>
                                        <Label bsStyle="info">{news.time}</Label>
                                    </Media.Body>
                                </Link>
                            </Media.ListItem>
                        ))}
                    </Media.List>
                </Media.Body>
            </Media>
        );
    }
}