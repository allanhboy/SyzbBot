import * as React from 'react';
import { Route } from 'react-router-dom';
import Loadable from 'react-loadable';

import Layout from './components/Layout';
import Loading from './components/Loading';

export const routes = <Layout>
    <Route exact strict path='/:tag' component={Loadable({
        loader: () => import('./components/Home'),
        loading: Loading
    })} />
    <Route exact path='/news/:id' component={Loadable({
        loader: () => import('./components/NewsDetail'),
        loading: Loading
    })} />
</Layout>;