using CBG;
using Persistence.User;

namespace Persistence
{
	public class UnitOfWork :
		UnitOfWork<DatabaseContext>, IUnitOfWork
	{
		public UnitOfWork(DatabaseContext databaseContext) : base(databaseContext: databaseContext)
		{
		}

		// **********
		private IUserRepository _userRepository;

		public IUserRepository UserRepository
		{
			get
			{
				if (_userRepository == null)
				{
					_userRepository =
						new UserRepository(databaseContext: DatabaseContext);
				}

				return _userRepository;
			}
		}
		// **********
	}
}
