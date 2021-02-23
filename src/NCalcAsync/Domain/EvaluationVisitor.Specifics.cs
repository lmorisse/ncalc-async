using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NCalcAsync.Domain
{
    public partial class EvaluationVisitor
    {
        /// <summary>
        ///     STEP built in function
        ///     STEP: Generate a step increase(or decrease) at the given time
        ///     Parameters: 2: (height, start time); step up/ down at start time
        /// </summary>
        /// <example>STEP(6, 3) steps from 0 to 6 at time 3(and stays there)</example>
        private async Task VisitStep(Function function)
        {
            if (function.Expressions.Length != 2)
                throw new ArgumentException("Step() takes exactly 2 arguments");
            if (!_time.HasValue)
                throw new ArgumentException("Time must not be null");
            var height = Convert.ToSingle(await EvaluateAsync(function.Expressions[0]));
            var startTime = Convert.ToUInt32(await EvaluateAsync(function.Expressions[1]));

            //return sim.Time >= startTime ? GetValue(0, selfVariable, variables, sim) : 0;
            Result = _time >= startTime ? height : 0;
        }
    }
}