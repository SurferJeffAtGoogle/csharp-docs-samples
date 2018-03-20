// Copyright (c) 2018 Google LLC.
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

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sudokumb;
using WebApp.Models;
using WebApp.Models.SudokumbViewModels;
using WebApp.Services;

namespace WebApp.Controllers
{
    public class SudokumbController : Controller
    {
        readonly IGameBoardQueue _gameBoardQueue;
        readonly AdminSettings _adminSettings;
        readonly SolveStateStore _solveStateStore;

        public SudokumbController(IGameBoardQueue gameBoardQueue,
            AdminSettings adminSettings,
            SolveStateStore solveStateStore)
        {
            _gameBoardQueue = gameBoardQueue;
            _adminSettings = adminSettings;
            _solveStateStore = solveStateStore;
        }

        public IActionResult Index()
        {
            // Show the user a puzzle by default.
            var model = new IndexViewModel
            {
                Form = new IndexViewForm()
                {
                    Puzzle = IndexViewForm.SamplePuzzle
                }
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(IndexViewForm form,
            CancellationToken cancellationToken)
        {
            var model = new IndexViewModel { Form = form };
            if (ModelState.IsValid)
            {
                // Solve the puzzle.
                GameBoard board = GameBoard.ParseHandInput(form.Puzzle);
                model.SolveRequestId = await _gameBoardQueue.StartSolving(
                    board, cancellationToken);
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Solve(string id,
            CancellationToken cancellationToken)
        {
            SolveState state = await _solveStateStore.GetAsync(id,
                cancellationToken);
            return new JsonResult(new
            {
                BoardsExaminedCount = state.BoardsExaminedCount,
                Solution = state.Solution?.ToHandInputString()
            });
        }


        [HttpGet]
        [Authorize(Roles="admin")]
        public async Task<IActionResult> Admin()
        {
            AdminViewModel model = new AdminViewModel()
            {
                Dumb = await _adminSettings.IsDumbAsync()
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles="admin")]
        public async Task<IActionResult> Admin(AdminViewModel model)
        {
            await _adminSettings.SetDumbAsync(model.Dumb);
            return View(model);
        }

        public IActionResult ThrowError()
        {
            throw new Exception("Don't visit /ThrowError!");
        }
    }
}
