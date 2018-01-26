﻿const path = require('path');
const webpack = require('webpack');
const ExtractTextPlugin = require('extract-text-webpack-plugin');
//const CheckerPlugin = require('awesome-typescript-loader').CheckerPlugin;
const bundleOutputDir = './wwwroot/dist';

module.exports = (env) => {
	const isDevBuild = !(env && env.prod);
	return [{
		stats: { modules: false },
		entry: { 'main': './ClientApp/boot.jsx' },
		resolve: { extensions: ['.js', '.jsx'] },
		output: {
			path: path.join(__dirname, bundleOutputDir),
			filename: '[name].js',
			publicPath: 'dist/'
		},
		module: {
			rules: [
				//{ test: /\.tsx?$/, include: /ClientApp/, use: 'awesome-typescript-loader?silent=true&useBabel=true' },
				{ test: /\.jsx?$/, use: 'babel-loader'},
				{ test: /\.less$/, use: isDevBuild ? ['style-loader', 'css-loader', 'less-loader'] : ExtractTextPlugin.extract({ use: 'css-loader?minimize!less-loader' }) },
				{ test: /\.(png|jpg|jpeg|gif|svg)$/, use: 'url-loader?limit=25000' }
			]
		},
		plugins: [
			//new CheckerPlugin(),
			new webpack.DllReferencePlugin({
				context: __dirname,
				manifest: require('./wwwroot/dist/vendor-manifest.json')
			})
		].concat(isDevBuild ? [
			// Plugins that apply in development builds only
			new webpack.SourceMapDevToolPlugin({
				filename: '[file].map', // Remove this line if you prefer inline source maps
				moduleFilenameTemplate: path.relative(bundleOutputDir, '[resourcePath]') // Point sourcemap entries to the original file locations on disk
			})
		] : [
			// Plugins that apply in production builds only
			new webpack.optimize.UglifyJsPlugin(),
			new ExtractTextPlugin('site.css')
		])
	}];
};