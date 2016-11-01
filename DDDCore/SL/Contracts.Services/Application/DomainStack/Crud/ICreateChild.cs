using System.Threading.Tasks;

namespace Contracts.Services.Application.DomainStack.Crud
{
    public interface ICreateChild<TVm, in TParrentKey, in TIm>
    {
        Task<TVm> CreateChildAsync(TParrentKey key, TIm im);
    }
}