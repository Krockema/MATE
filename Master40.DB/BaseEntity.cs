using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Reflection;
using Master40.DB.Data.Helper;
using Master40.DB.Data.WrappersForPrimitives;

namespace Master40.DB
{
    public class BaseEntity : IBaseEntity
    {
        // [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        protected BaseEntity()
        {
            if (Configuration.TrackObjects)
            {
                // create id && track callers of this constructor
            
                // 1st frame should be the constructor calling
                // 2nd frame should be the constructor of the inherited class
                MethodBase methodFromCaller = new StackFrame(1).GetMethod();
                StackFrame stackFrameFromRequester = new StackFrame(2);
                MethodBase methodFromRequester = stackFrameFromRequester.GetMethod();

                // return the type of the inherited class
                Type caller = null;
                if (methodFromCaller != null && methodFromCaller.ReflectedType != null)
                {
                    caller = methodFromCaller.ReflectedType;
                }

                string requester = "";
                if (methodFromRequester != null && methodFromRequester.ReflectedType != null)
                {
                    requester = $"{methodFromRequester.ReflectedType.Name}: ";
                }
                if (methodFromRequester != null)
                {
                    requester += methodFromRequester.Name;   
                }
                
                Id = IdGeneratorHolder.GetIdGenerator().GetNewId(caller,requester).GetValue();
            }
            else
            {
                Id = IdGeneratorHolder.GetIdGenerator().GetNewId().GetValue();
            }

            
        }
        
        public Id GetId()
        {
            return new Id(Id);
        }

        public override bool Equals(object obj)
        {
            BaseEntity other = (BaseEntity)obj;
            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

}