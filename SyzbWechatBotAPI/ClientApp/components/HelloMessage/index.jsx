import './hellomessage.less';
import * as React from 'react';

export default class HelloMessage extends React.Component {
	render() {
		return <h1>Hello {this.props.message} !</h1>;
	}
}