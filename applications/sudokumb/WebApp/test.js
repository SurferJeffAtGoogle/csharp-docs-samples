// Copyright(c) 2017 Google Inc.
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
console.log(host);

casper.test.begin('Visit pages.', 10, function suite(test) {
    // Click the links in the top nav one by one.
    casper.start(host + '/', function (response) {
        this.echo('Starting ' + response.url);
        test.assertEquals(200, response.status);
        test.assertSelectorHasText('title', 'Home Page - Sudokumb');
    });

    casper.thenClick('#nav-solve', function(response) {
        this.echo('Visiting ' + response.url);        
        test.assertEquals(200, response.status);
        test.assertSelectorHasText('title', 'Solve - Sudokumb');
    });

    casper.thenClick('#nav-admin', function(response) {
        this.echo('Visiting ' + response.url);        
        test.assertEquals(200, response.status);
        test.assertSelectorHasText('title', 'Log in - Sudokumb');
    });

    casper.thenClick('#nav-home', function(response) {
        this.echo('Visiting ' + response.url);        
        test.assertEquals(200, response.status);
        test.assertSelectorHasText('title', 'Home Page - Sudokumb');
    });

    // Then click the Try It! button.
    casper.thenClick('.btn-primary', function(response) {
        this.echo('Visiting ' + response.url);        
        test.assertEquals(200, response.status);
        test.assertSelectorHasText('title', 'Solve - Sudokumb');
    });
    
    /*
    casper.thenClick('#Submit', function (response) {
        this.echo('Submitted form.');
        test.assertEquals(200, response.status);
        test.assertSelectorHasText('#LastIncrementedBy', 'test.js');
        test.assertSelectorHasText('#CurrentCount', currentCount + 1);
        this.fill('#WhoForm', {
            'Who': 'anothertest.js',
        }, false);
        this.echo("Filled another form.");
    });

    casper.thenClick('#Submit', function (response) {
        this.echo('Submitted form.');
        test.assertEquals(200, response.status);
        test.assertSelectorHasText('#LastIncrementedBy', 'anothertest.js');
    });

    casper.thenClick('#Reset', function (response) {
        this.echo('Clicked Reset.');
        test.assertEquals(200, response.status);
        test.assertSelectorHasText('#CurrentCount', '0');
    });
    */

    casper.run(function () {
        test.done();
    });
});

casper.test.begin('Submit puzzle', 3, function suite(test) {
    casper.start(host + '/').thenClick('#nav-solve', function(response) {
        this.echo('Visiting ' + response.url);        
        test.assertEquals(200, response.status);
    });

    // Then click the Submit button.
    casper.thenClick('.btn-primary', function(response) {
        this.echo('Visiting ' + response.url);        
        test.assertEquals(200, response.status);
        test.assertSelectorHasText('#solvingMessage', 'Solving...');
    });

    casper.waitForSelectorTextChange('#solvingMessage', function() {
        this.echo("Yeah, it's really solving.");
    }, function() {
        test.fail("Never updated solving status message.");
    }, 4000);
    
    casper.run(function () {
        test.done();
    });
});