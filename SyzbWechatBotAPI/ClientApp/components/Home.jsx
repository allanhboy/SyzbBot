import * as React from 'react';

import NewsList from "./NewsList";


export default class Home extends React.Component {
    render() {
        const { match: { params: { tag } } } = this.props;
        return (
            <NewsList tag={tag} />
        );
    }
}