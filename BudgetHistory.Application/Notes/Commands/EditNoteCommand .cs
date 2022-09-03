﻿using BudgetHistory.Application.Core;
using BudgetHistory.Application.DTOs;
using MediatR;

namespace BudgetHistory.Application.Notes.Commands
{
    public class EditNoteCommand : IRequest<Result<string>>
    {
        public NoteDto EditedNote { get; set; }
    }
}