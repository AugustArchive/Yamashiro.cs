using System.Collections.Generic;
using System.Threading.Tasks;

namespace Yamashiro.Infrastructure.Conditions
{
    public class ConditionList<T>: BaseCondition<T>
    {
        private readonly List<BaseCondition<T>> conditions = new List<BaseCondition<T>>();
        public BaseCondition<T> Add(BaseCondition<T> condition)
        {
            this.conditions.Add(condition);
            return condition;
        }

        public async Task<bool> PreconditionAsync(YamashiroCommandContext ctx, T arg)
        {
            foreach (var condition in conditions)
            {
                var result = await condition.PreconditionAsync(ctx, arg).ConfigureAwait(false);
                if (!result) return false;
            }

            return true;
        }
    }
}