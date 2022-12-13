using BudgetHistory.Application.DTOs.Note;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Resources;
using FluentValidation;
using System;
using System.Linq;

namespace BudgetHistory.Application.Notes.Validators
{
    public class BaseNoteDtoValidator : AbstractValidator<NoteDto>
    {
        public BaseNoteDtoValidator()
        {
            RuleFor(note => note.UserId).NotEmpty();
            RuleFor(note => note.RoomId).NotEmpty();
            RuleFor(note => note.Value).NotEmpty();
            RuleFor(note => note.Currency).NotEmpty().Must(currency => Enum.GetNames<Currency>().Contains(currency)).WithMessage(ResponseMessages.InvalidCurrencyType);
            RuleFor(note => note.Comment.Length).LessThan(1000);
        }
    }
}