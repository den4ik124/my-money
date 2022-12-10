using AutoMapper;
using BudgetHistory.Abstractions.Interfaces.Data;
using BudgetHistory.Application.Core;
using BudgetHistory.Application.DTOs.Auth;
using BudgetHistory.Auth.Interfaces;
using BudgetHistory.Core.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BudgetHistory.Application.Auth.Queries
{
    public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<UserDataDto>>
    {
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public GetCurrentUserQueryHandler(IAuthService authService, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _authService = authService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<UserDataDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.UserName))
            {
                return Result<UserDataDto>.Failure("'UserName' is empty. Such user does not exists");
            }
            var result = await _authService.GetCurrentUser(request.UserName);
            if (!result.IsSuccess)
            {
                return Result<UserDataDto>.Failure(result.Message);
            }

            var identityUser = result.Value;

            var user = await _unitOfWork.GetGenericRepository<User>().GetFirst(u => u.AssociatedIdentityUserId.ToString() == identityUser.Id);
            var rolesResult = await _authService.GetUserRoles(identityUser);
            if (!rolesResult.IsSuccess)
            {
                return Result<UserDataDto>.Failure(rolesResult.Message);
            }

            var userDataDtoResponse = _mapper.Map<User, UserDataDto>(user); //TODO нет ролей. Добавить проверку
            userDataDtoResponse.Roles = rolesResult.Value;

            return Result<UserDataDto>.Success(userDataDtoResponse);
        }
    }
}