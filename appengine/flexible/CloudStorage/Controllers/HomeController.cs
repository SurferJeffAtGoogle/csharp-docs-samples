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

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using CloudStorage.ViewModels;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Google.Cloud.Storage.V1;
using System.Text;
using Microsoft.Extensions.Options;
using Google;

namespace CloudStorage.Controllers
{
    public class HomeController : Controller
    {
        readonly CloudStorageOptions _options;
        readonly StorageClient _storage;
        public HomeController(IOptions<CloudStorageOptions> options)
        {
            _options = options.Value;
            _storage = StorageClient.Create();
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.Headers = this.HttpContext.Request.Headers;

            var model = new HomeIndex();
            if (new string[] { null, "", "your-google-bucket-name" }
                .Contains(_options.BucketName))
            {
                model.MissingBucketName = true;
                return View(model);
            }
            try {
                var storageObject = 
                    await _storage.GetObjectAsync(_options.BucketName, _options.ObjectName);
                model.MediaLink = storageObject.MediaLink;
                MemoryStream m = new MemoryStream();
                await _storage.DownloadObjectAsync(
                    _options.BucketName, _options.ObjectName, m);
                m.Seek(0, SeekOrigin.Begin);
                byte[] content = new byte[m.Length];
                m.Read(content, 0, content.Length);
                model.Content = Encoding.UTF8.GetString(content);
            }
            catch (GoogleApiException e)
            when (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Does not exist yet.  No problem.
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(Form sendForm)
        {
            var model = new HomeIndex();
            await _storage.UploadObjectAsync(
                _options.BucketName, _options.ObjectName, "text/plain",
                new MemoryStream(Encoding.UTF8.GetBytes(sendForm.Content)));
            model.Content = sendForm.Content;
            model.SavedNewContent = true;
            var storageObject =
                await _storage.GetObjectAsync(_options.BucketName, _options.ObjectName);
            model.MediaLink = storageObject.MediaLink;
            return View(model);
        }


        public IActionResult Error()
        {
            return View();
        }
    }
}
