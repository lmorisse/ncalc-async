using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathNet.Numerics.Distributions;

namespace NCalcAsync.Domain
{
    public partial class EvaluationVisitor
    {
        public const float Tolerance = 0.001f;
        private readonly ushort? _time;
        private readonly uint? _step;
        private readonly float? _deltaTime;
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

            Result = _time >= startTime ? height : 0;
        }

        /// <summary>
        ///     Ramp built in function
        ///     Generates a ramp of slope slope, starting at time time and zero before that time.
        ///     ramp(time,slope)
        ///     Arguments: time at which to start ramping, slope (positive or negative) of ramp
        /// </summary>
        /// <example>ramp(20,-7) will have a return value of 0 at time 20 and -70 at time 30</example>
        private async Task VisitRamp(Function function)
        {
            if (function.Expressions.Length != 2)
                throw new ArgumentException("Ramp() takes exactly 2 arguments");
            if (!_time.HasValue)
                throw new ArgumentException("Time must not be null");
            var time = Convert.ToUInt16(await EvaluateAsync(function.Expressions[0]));
            var slope = Convert.ToSingle(await EvaluateAsync(function.Expressions[1]));

            Result = _time >= time ? slope * (_time - time) : 0;
        }

        /// <summary>
        ///     Pulse built in function
        ///     Generate a one-DT wide pulse at the given time
        ///     Parameters: 2 or 3: (magnitude, first time[, interval])
        ///     Without interval or when interval = 0, the PULSE is generated only once
        /// </summary>
        /// <example>PULSE(20, 12, 5) generates a pulse value of 20/DT at time 12, 17, 22, etc</example>
        private async Task VisitPulse(Function function)
        {
            if (!_step.HasValue)
                throw new ArgumentException("Step must not be null");
            var magnitude = Convert.ToSingle(await EvaluateAsync(function.Expressions[0]));
            var firstTime = Convert.ToUInt16(await EvaluateAsync(function.Expressions[1]));
            var interval = function.Expressions.Length == 2
                ? 0
                : Convert.ToUInt16(await EvaluateAsync(function.Expressions[2]));
            var delta = Convert.ToSingle(firstTime - _step * _deltaTime);
            if (interval == 0)
            {
                Result = Math.Abs(delta) < Tolerance
                    ? magnitude
                    : 0;
            }
            else
            {
                Result = Math.Abs(delta % interval) < Tolerance
                    ? magnitude
                    : 0;
            }
        }

        /// <summary>
        ///     Normal built in function
        ///     Sample a value from a Normal distribution
        ///     Parameters: 2 or 3: (mean, standard deviation[, seed]);
        ///     Range [0;  232[
        ///     If seed is provided, the sequence of numbers will always be identical
        /// </summary>
        /// <example>NORMAL(100, 5) samples from N(100, 5)</example>
        private async Task VisitNormal(Function function)
        {
            var mean = Convert.ToSingle(await EvaluateAsync(function.Expressions[0]));
            var standardDeviation = Convert.ToSingle(await EvaluateAsync(function.Expressions[1]));
            var seed = function.Expressions.Length == 2
                ? 0
                : Convert.ToInt32(await EvaluateAsync(function.Expressions[2]));
            if (seed == 0)
            {
                Result = Convert.ToSingle(Normal.Sample(mean, standardDeviation));
            }
            else
            {
                var random = new Random(seed);
                Result = Convert.ToSingle(Normal.Sample(random, mean, standardDeviation));
            }
        }

        /// <summary>
        ///     ExternalUpdate built in function
        ///     Used when a variable is updated at each step via an external device
        ///     The variable has no equation so it could be removed by the optimizer
        ///     Using this function will avoid this pitfall
        ///     Parameter : InitialValue (Optional)
        /// </summary>
        /// <example>new Variable("example", "ExternalUpdate(1)")</example>
        private async Task VisitExternalUpdate(Function function)
        {
            if (!_step.HasValue)
                throw new ArgumentException("Step must not be null");
            Result = _step == 0 && function.Expressions.Length == 1 ? Convert.ToSingle(await EvaluateAsync(function.Expressions[0])) :
                function.LastValue??0;
        }
        /// <summary>
        ///     Value built in function
        ///     Set the value of a variable when the function is called. This value won't evolve even if the variable value does.
        ///     Used with functions that are triggered by a starttime such as Step, Ramp, Pulse, ...
        ///     value(variableId)
        ///     Arguments: variableId
        /// </summary>
        /// <example>Value(Time) will have a return value of 1 if called at time 1 or after</example>
        private async Task VisitValue(Function function)
        {
            Result = function.LastValue ?? await EvaluateAsync(function.Expressions[0]);
        }
        #region SMTH

        /// <example>Smth1(Input, Averaging, initialValue(Option)) </example>
        private async Task VisitSmth1(Function function)
        {
            Result = await VisitSmth(function, 1);
        }
        /// <example>Smth3(Input, Averaging, initialValue(Option)) </example>
        private async Task VisitSmth3(Function function)
        {
            Result = await VisitSmth(function, 3);
        }
        /// <example>SmthN(Input, Averaging, Order, initialValue(Option)) </example>
        private async Task VisitSmthN(Function function)
        {
            var order = Convert.ToInt32(await EvaluateAsync(function.Expressions[2]));
            Result = await VisitSmth(function, order,4);
        }
        /// <summary>
        ///     The smth1, smth3 and smthn functions perform a first-, third- and nth-order respectively exponential smooth of
        ///     input, using an exponential averaging time of averaging,
        ///     and an optional initial value initial for the smooth.smth3 does this by setting up a cascade of three first-order
        ///     exponential smooths, each with an averaging time of averaging/3.
        ///     The other functions behave analogously.They return the value of the final smooth in the cascade.
        ///     If you do not specify an initial value initial, they assume the value to be the initial value of input.
        /// </summary>
        /// <example>Smth(Input, Averaging, order, initialindex(Option)) </example>
        private async Task<float> VisitSmth(Function function, int order, byte maxParam=3)
        {
            if (!_deltaTime.HasValue)
                throw new ArgumentException("DeltaTime must not be null");
            if (!_step.HasValue)
                throw new ArgumentException("Step must not be null");
            var previousValues = new float[order];
            var initialIndex = function.Expressions.Length == maxParam ? maxParam-1 : 0;
            var initial = Convert.ToSingle(await EvaluateAsync(function.Expressions[initialIndex]));

            previousValues[0] = initial;

            for (var i = 1; i < order; i++)
            {
                previousValues[i] = previousValues[0];
            }
            if (_step == 0)
            {
                if (function.Expressions.Length == maxParam)
                {
                    return previousValues.Last();
                }
                // If initial value is not setted, we add the expression at step 0. Otherwise, initial value will evolve with time
                var initialExpression = new ValueExpression(initial);
                var expressions = function.Expressions.ToList();
                expressions.Add(initialExpression);
                function.Expressions = expressions.ToArray();

                return previousValues.Last();
            }

            var input = Convert.ToSingle(await EvaluateAsync(function.Expressions[0]));
            var averaging = Convert.ToSingle(await EvaluateAsync(function.Expressions[1]));
            for (var t = 0; t < _step; t++)
            {
                for (var i = 0; i < order; i++)
                {
                    previousValues[i] += _deltaTime.Value * (input - previousValues[i]) * order / averaging;
                    input = previousValues[i];
                }
            }
            
            return previousValues.Last();
        }
        #endregion
    }
}