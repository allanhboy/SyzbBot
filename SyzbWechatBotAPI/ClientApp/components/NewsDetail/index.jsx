import './news.less';
import * as React from 'react';
import fetch from "isomorphic-fetch";
import Jumbotron from "react-bootstrap/lib/Jumbotron";
import Label from 'react-bootstrap/lib/Label';

export default class News extends React.Component {
    constructor() {
        super();
        this.state = {
            news: {}, loading: true
        };
    }

    componentDidMount() {
        const { match: { params: { id } } } = this.props;
        fetch(`/api/News?id=${id}`)
            .then(response => response.json())
            .then(json => {
                this.setState({
                    news: json, loading: false
                });
            });
    }

    render() {
        const { news } = this.state;
        return (
            <Jumbotron>
                <h1>{news.title}</h1>
                <Label bsStyle="info">{news.time}</Label>
                <p>{news.text}</p>
            </Jumbotron>
        );
    }
}