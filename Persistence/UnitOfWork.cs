using CBG;
using Persistence.User;
using Persistence.Tool;
using Persistence.Page;

namespace Persistence
{
    public class UnitOfWork : UnitOfWork<DatabaseContext>, IUnitOfWork
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
                    _userRepository = new UserRepository(databaseContext: DatabaseContext);
                }

                return _userRepository;
            }
        }
        // **********

        // **********
        private IToolRepository _toolRepository;

        public IToolRepository ToolRepository
        {
            get
            {
                if (_toolRepository == null)
                {
                    _toolRepository = new ToolRepository(databaseContext: DatabaseContext);
                }

                return _toolRepository;
            }
        }
        // **********

        // **********
        private IPageRepository _pageRepository;

        public IPageRepository PageRepository
        {
            get
            {
                if (_pageRepository == null)
                {
                    _pageRepository = new PageRepository(databaseContext: DatabaseContext);
                }

                return _pageRepository;
            }
        }
        // **********
    }
}
