using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatisfactoryModdingHelper.Models
{
//[AccessTransformers]
//Friend=(Class="AFGSomeClass", FriendClass="UMyClass")
//Accessor=(Class="AFGSomeClass", Property="mSomeProperty")
//BlueprintReadWrite=(Class="AFGSomeClass", Property="mSomeProperty")
    public class AccessTransformersModel : IEquatable<AccessTransformersModel>
    {
        public AccessTransformersModel()
        {
            AccessorTransformers = new ObservableCollection<AccessorModel>();
            FriendTransformers = new ObservableCollection<FriendModel>();
            BlueprintReadWriteTransformers = new ObservableCollection<BlueprintReadWriteModel>();
        }

        public ObservableCollection<FriendModel> FriendTransformers;
        public ObservableCollection<AccessorModel> AccessorTransformers;
        public ObservableCollection<BlueprintReadWriteModel> BlueprintReadWriteTransformers;
        public bool Equals(AccessTransformersModel other)
        {
            bool friendEq = false;
            if (this.FriendTransformers != null && other.FriendTransformers != null)
            {
                friendEq = Enumerable.SequenceEqual(this.FriendTransformers, other.FriendTransformers);
            }
            else if (this.FriendTransformers == null && other.FriendTransformers == null)
            {
                friendEq = true;
            }
            bool accessorEq = false;
            if (this.AccessorTransformers != null && other.AccessorTransformers != null)
            {
                accessorEq = Enumerable.SequenceEqual(this.AccessorTransformers, other.AccessorTransformers);
            }
            else if (this.AccessorTransformers == null && other.AccessorTransformers == null)
            {
                accessorEq = true;
            }
            bool blueprintReadWriteEq = false;
            if (this.BlueprintReadWriteTransformers != null && other.BlueprintReadWriteTransformers != null)
            {
                blueprintReadWriteEq = Enumerable.SequenceEqual(this.BlueprintReadWriteTransformers, other.BlueprintReadWriteTransformers);
            }
            else if (this.BlueprintReadWriteTransformers == null && other.BlueprintReadWriteTransformers == null)
            {
                blueprintReadWriteEq = true;
            }
            return friendEq && accessorEq && blueprintReadWriteEq;
        }
    }

    public class FriendModel : IEquatable<FriendModel>
    {
        public FriendModel()
        {
            Class = "";
            FriendClass = "";
        }
        public string Class { get; set; }
        public string FriendClass { get; set; }
        public bool Equals(FriendModel other)
        {
            var thisValues = this.GetType().GetProperties().Select(p => p.GetValue(this))
    .Select(o => Object.ReferenceEquals(o, null) ? default(string) : o.ToString());
            var otherValues = other.GetType().GetProperties().Select(p => p.GetValue(other))
                .Select(o => Object.ReferenceEquals(o, null) ? default(string) : o.ToString());
            return Enumerable.SequenceEqual(thisValues, otherValues);
        }
    }

    public class AccessorModel : IEquatable<AccessorModel>
    {
        public AccessorModel()
        {
            Class = "";
            Property = "";
        }
        public string Class { get; set; }
        public string Property { get; set; }
        public bool Equals(AccessorModel other)
        {
            var thisValues = this.GetType().GetProperties().Select(p => p.GetValue(this))
    .Select(o => Object.ReferenceEquals(o, null) ? default(string) : o.ToString());
            var otherValues = other.GetType().GetProperties().Select(p => p.GetValue(other))
                .Select(o => Object.ReferenceEquals(o, null) ? default(string) : o.ToString());
            return Enumerable.SequenceEqual(thisValues, otherValues);
        }
    }

    public class BlueprintReadWriteModel : IEquatable<BlueprintReadWriteModel>
    {
        public BlueprintReadWriteModel()
        {
            Class = "";
            Property = "";
        }
        public string Class { get; set; }
        public string Property { get; set; }
        public bool Equals(BlueprintReadWriteModel other)
        {
            var thisValues = this.GetType().GetProperties().Select(p => p.GetValue(this))
    .Select(o => Object.ReferenceEquals(o, null) ? default(string) : o.ToString());
            var otherValues = other.GetType().GetProperties().Select(p => p.GetValue(other))
                .Select(o => Object.ReferenceEquals(o, null) ? default(string) : o.ToString());
            return Enumerable.SequenceEqual(thisValues, otherValues);
        }
    }
}
