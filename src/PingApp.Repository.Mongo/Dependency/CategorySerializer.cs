using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using PingApp.Entity;

namespace PingApp.Repository.Mongo.Dependency {
    sealed class CategorySerializer : BsonBaseSerializer {
        public override void Serialize(BsonWriter bsonWriter, 
            Type nominalType, object value, IBsonSerializationOptions options) {
                Category category = value as Category;
                bsonWriter.WriteInt32(category.Id);
        }

        public override object Deserialize(BsonReader bsonReader,
            Type nominalType, Type actualType, IBsonSerializationOptions options) {
            if (nominalType != typeof(Category) || actualType != typeof(Category)) {
                throw new BsonSerializationException("This serializer can only serialize type PingApp.Entity.Category");
            }

            if (bsonReader.GetCurrentBsonType() != BsonType.Int32) {
                throw new FormatException("Category should be serialized to Int32");
            }

            int id = bsonReader.ReadInt32();
            return Category.Get(id);
        }
    }
}
