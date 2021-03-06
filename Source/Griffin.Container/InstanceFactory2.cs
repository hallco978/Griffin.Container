﻿using System.Linq.Expressions;
using System.Reflection;

namespace Griffin.Container
{
    /// <summary>
    /// Delegate used to create objects
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public delegate object ObjectActivator(params object[] args);

    /// <summary>
    /// Credits http://rogeralsing.com/2008/02/28/linq-expressions-creating-objects/
    /// </summary>
    public static class ConstructorExtensions
    {
        /// <summary>
        /// Gets activator (instance factory method).
        /// </summary>
        /// <param name="ctor">The ctor.</param>
        /// <returns>The activator</returns>
        public static ObjectActivator GetActivator(this ConstructorInfo ctor)
        {
            var paramsInfo = ctor.GetParameters();
            var param = Expression.Parameter(typeof (object[]), "args");

            var argsExp = new Expression[paramsInfo.Length];

            //pick each arg from the params array 
            //and create a typed expression of them
            for (var i = 0; i < paramsInfo.Length; i++)
            {
                Expression index = Expression.Constant(i);
                var paramType = paramsInfo[i].ParameterType;
                Expression paramAccessorExp = Expression.ArrayIndex(param, index);
                Expression paramCastExp = Expression.Convert(paramAccessorExp, paramType);
                argsExp[i] = paramCastExp;
            }

            //make a NewExpression that calls the
            //ctor with the args we just created
            var newExp = Expression.New(ctor, argsExp);

            //create a lambda with the New
            //Expression as body and our param object[] as arg
            var lambda = Expression.Lambda(typeof (ObjectActivator), newExp, param);

            //compile it
            return (ObjectActivator) lambda.Compile();
        }
    }
}