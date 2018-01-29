import * as React from 'react';
import { Route } from 'react-router-dom';
import Loadable from 'react-loadable';

import Layout from './components/Layout';
import Loading from './components/Loading';

export const routes = <Layout>
    <Route exact path='/:tag' component={Loadable({
        loader: () => import('./components/Home'),
        loading: Loading
    })} />
</Layout>;