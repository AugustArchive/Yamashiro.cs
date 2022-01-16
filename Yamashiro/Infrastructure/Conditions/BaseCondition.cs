using System.Threading.Tasks;

namespace Yamashiro.Infrastructure.Conditions
{
    public interface BaseCondition<in T>
    {
        /// <summary>
        /// Makes sure that this condition is successful
        /// </summary>
        /// <param name="ctx">The command context</param>
        /// <param name="arg">The argument itself</param>
        /// <returns>A boolean if the condition was successful</returns>
        Task<bool> PreconditionAsync(YamashiroCommandContext ctx, T arg);
    }
}