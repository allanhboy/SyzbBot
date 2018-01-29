import * as React from 'react';
import PageHeader from "react-bootstrap/lib/PageHeader";
import NewsList from "./NewsList";


export default class Home extends React.Component {
    render() {
        const { match: { params: { tag } } } = this.props;
        return (
            <div>
                <PageHeader>
                    <img src="/images/logo.png" />
                </PageHeader>
                <NewsList tag={tag} />
            </div>
        );
    }
}