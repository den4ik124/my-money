using BudgetHistory.Abstractions.Interfaces.Data;
using BudgetHistory.Abstractions.Interfaces.Services;
using BudgetHistory.Auth.Interfaces;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Resources;
using BudgetHistory.Core.Services.Responses;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace BudgetHistory.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IAuthService _authService;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IAuthService authService, IUnitOfWork unitOfWork)
        {
            _authService = authService;
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResponse<User>> GetCurrentUser(IdentityUser identityUser)
        {
            var user = await _unitOfWork.GetGenericRepository<User>().GetFirst(u => u.AssociatedIdentityUserId.ToString() == identityUser.Id);
            if (user is null)
            {
                //TODO : logging here
                return ServiceResponse<User>.Failure(ResponseMessages.UserWithNameDoesNotExist);
            }

            var userRolesResult = await _authService.GetUserRoles(identityUser);
            if (userRolesResult.IsSuccess is false)
            {
                return ServiceResponse<User>.Failure(userRolesResult.Message);
            }
            user.Roles = userRolesResult.Value;
            return ServiceResponse<User>.Success(user);
        }

        //TODO : Get all users with roles
    }
}