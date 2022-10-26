using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Core.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BudgetHistory.Tests.Mocks
{
    public static class MockRepository
    {
        public static Mock<IGenericRepository<Note>> GetMockedNoteRepository()
        {
            var items = new List<Note>()
            {
                new Note()
                {
                    Id = Guid.NewGuid(),
                },
                new Note()
                {
                    Id = Guid.NewGuid(),
                },
                new Note()
                {
                    Id = Guid.NewGuid(),
                },
            };
            return RepositoryCreation(items);
        }

        public static Mock<IGenericRepository<Room>> GetMockedRoomRepository()
        {
            var password = "pa$$word123";
            var encryptedPassword = "49pvlHnhaTRUmNU9oZFo8A==";
            var items = new List<Room>()
            {
                new Room()
                {
                    Id = Guid.NewGuid(),
                    Password = password,
                    EncryptedPassword = encryptedPassword
                },
                new Room()
                {
                    Id = Guid.NewGuid(),
                    Password = password,
                    EncryptedPassword = encryptedPassword
                },
                new Room()
                {
                    Id = Guid.NewGuid(),
                    Password = password,
                    EncryptedPassword = encryptedPassword
                },
            };
            return RepositoryCreation(items);
        }

        private static Mock<IGenericRepository<T>> RepositoryCreation<T>(ICollection<T> items) where T : class
        {
            var mockRepo = new Mock<IGenericRepository<T>>();
            mockRepo.Setup(rep => rep.GetAll()).ReturnsAsync(items);

            mockRepo.Setup(rep => rep.Add(It.IsAny<T>())).ReturnsAsync((T item) =>
            {
                items.Add(item);
                return true;
            });

            mockRepo.Setup(rep => rep.Delete(It.IsAny<T>())).Returns((T item) =>
            {
                items.Remove(item);
                return true;
            });

            mockRepo.Setup(rep => rep.GetWhere(It.IsAny<Expression<Func<T, bool>>>())).ReturnsAsync((Expression<Func<T, bool>> predicate) =>
            {
                return items.AsQueryable().Where(predicate).ToList();
            });

            mockRepo.Setup(rep => rep.GetItemsCount(It.IsAny<Expression<Func<T, bool>>>())).ReturnsAsync((Expression<Func<T, bool>> predicate) =>
            {
                if (predicate != null)
                {
                    return items.AsQueryable().Where(predicate).Count();
                }
                return items.Count;
            });

            mockRepo.Setup(rep => rep.GetQuery(It.IsAny<Expression<Func<T, bool>>>(), It.IsAny<Func<IQueryable<T>, IOrderedQueryable<T>>>()))
                .Returns((Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy) =>
                {
                    if (predicate != null)
                    {
                        return items.AsQueryable().Where(predicate);
                    }
                    if (orderBy != null)
                    {
                        return orderBy(items.AsQueryable());
                    }

                    return items.AsQueryable();
                });

            return mockRepo;
        }
    }
}