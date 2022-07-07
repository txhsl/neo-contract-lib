using System.ComponentModel;

namespace DataStructure
{
    public partial class DataStructure
    {
        [DisplayName("Fault")]
        public static event FaultEvent onFault;
        public delegate void FaultEvent(string message, params object[] paras);
    }
}
