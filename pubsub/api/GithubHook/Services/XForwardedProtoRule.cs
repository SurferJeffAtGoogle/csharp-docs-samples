﻿/*
 * Copyright (c) 2017 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy of
 * the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations under
 * the License.
 */

using Microsoft.AspNetCore.Rewrite;
using System.Linq;

namespace GithubHook
{
    /// <summary>
    /// A rule that inspect the X-Forwarded-Proto header.  When the header is
    /// https, modifies the request to look like https.
    /// </summary>
    public class XForwardedProtoRule : IRule
    {
        void IRule.ApplyRule(RewriteContext context)
        {
            var request = context.HttpContext.Request;
            if (request.Scheme == "https")
            {
                return;  // Already https.
            }
            string proto = request.Headers["X-Forwarded-Proto"]
                .FirstOrDefault();
            if (proto == "https")
            {
                request.IsHttps = true;
                request.Scheme = "https";
            }
        }
    }
}