using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Core.HelperModels
{
    public enum Language
    {
        English,
        Arabic,
        Spanish
    }
    public enum Visibility
    {
        Active,
        Inactive
    }
    public class ModelBase
    {
        public int Id { get; set; }
    }
}
