using Persistence.User;
using Persistence.Tool;
using Persistence.Page;
using Persistence.Camera;

namespace Persistence
{
    public interface IUnitOfWork : Application.Interfaces.IUnitOfWork
    {
        IUserRepository UserRepository { get; }
        IToolRepository ToolRepository { get; }
        IPageRepository PageRepository { get; }
        ICameraRepository CameraRepository { get; }
    }
}
