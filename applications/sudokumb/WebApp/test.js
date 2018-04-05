﻿// Copyright(c) 2017 Google Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not
// use this file except in compliance with the License. You may obtain a copy of
// the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
// License for the specific language governing permissions and limitations under
// the License.

var system = require('system');
var host = system.env['CASPERJS11_URL'];

currentCount = 0;

casper.test.begin('Visit pages.', 6, function suite(test) {
    casper.start(host + '/', function (response) {
        console.log('Starting ' + host + '/');
        test.assertEquals(200, response.status);
        test.assertSelectorHasText('H1', 'Sudokumb');
    });

    casper.thenClick('#nav-solve', function(response) {
        test.assertEquals(200, response.status);
        test.assertSelectorHasText('H1', 'Solve');
    });

    casper.thenClick('#nav-admin', function(response) {
        test.assertEquals(200, response.status);
        test.assertSelectorHasText('H1', 'Log in');
    });

    /*
    casper.thenClick('#Submit', function (response) {
        console.log('Submitted form.');
        test.assertEquals(200, response.status);
        test.assertSelectorHasText('#LastIncrementedBy', 'test.js');
        test.assertSelectorHasText('#CurrentCount', currentCount + 1);
        this.fill('#WhoForm', {
            'Who': 'anothertest.js',
        }, false);
        console.log("Filled another form.");
    });

    casper.thenClick('#Submit', function (response) {
        console.log('Submitted form.');
        test.assertEquals(200, response.status);
        test.assertSelectorHasText('#LastIncrementedBy', 'anothertest.js');
    });

    casper.thenClick('#Reset', function (response) {
        console.log('Clicked Reset.');
        test.assertEquals(200, response.status);
        test.assertSelectorHasText('#CurrentCount', '0');
    });

    casper.run(function () {
        test.done();
    });
    */
});