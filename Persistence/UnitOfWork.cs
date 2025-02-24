using CBG;
using Persistence.User;

namespace Persistence
{
	public class UnitOfWork :
		UnitOfWork<DataBaseContext>, IUnitOfWork
	{
		public UnitOfWork(DataBaseContext databaseContext) : base(databaseContext: databaseContext)
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
