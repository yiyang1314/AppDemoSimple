using System;

namespace Rxbit.Brigade.Models.Attributes
{
    public class ToolboxAttribute : Attribute
    {
        public String Title { get; }
        public Int32 Id { get; }


        public ToolboxAttribute(Int32 id, String title)
        {
            this.Id = id;
            this.Title = title;
        }
    }
}
