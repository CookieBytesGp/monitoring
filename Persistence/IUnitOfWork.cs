using Persistence.User;

namespace Persistence
{
	public interface IUnitOfWork : Application.Interfaces.IUnitOfWork
	{
		IUserRepository UserRepository { get; }
	}
}
