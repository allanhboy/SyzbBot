import * as React from 'react';
import PageHeader from "react-bootstrap/lib/PageHeader";

export default class Layout extends React.Component{
    render(){
        return <div className='container-fluid'>
                   <PageHeader>
                       <img src="/images/logo.png" />
                   </PageHeader>
            { this.props.children }
        </div>
    }
}