using AutoMapper;
using BudgetHistory.Abstractions.Interfaces.Data;
using BudgetHistory.Application.Core;
using BudgetHistory.Application.DTOs.Auth;
using BudgetHistory.Auth.Interfaces;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Resources;
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
                //TODO Logging here
                return Result<UserDataDto>.Failure(ResponseMessages.EmptyUserName);
            }
            var identityUserResult = await _authService.GetCurrentUser(request.UserName);
            if (identityUserResult.IsSuccess is false)
            {
                //TODO Logging here
                return Result<UserDataDto>.Failure(identityUserResult.Message);
            }

            var identityUser = identityUserResult.Value;

            var user = await _unitOfWork.GetGenericRepository<User>().GetFirst(u => u.AssociatedIdentityUserId.ToString() == identityUser.Id);
            var rolesResult = await _authService.GetUserRoles(identityUser);
            if (!rolesResult.IsSuccess)
            {
                return Result<UserDataDto>.Failure(rolesResult.Message);
            }

            var userDataDtoResponse = _mapper.Map<User, UserDataDto>(user);
            userDataDtoResponse.Roles = rolesResult.Value;

            return Result<UserDataDto>.Success(userDataDtoResponse);
        }
    }
}