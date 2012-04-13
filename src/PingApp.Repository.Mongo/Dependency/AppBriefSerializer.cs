using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using PingApp.Entity;

namespace PingApp.Repository.Mongo.Dependency {
    sealed class AppBriefSerializer : BsonBaseSerializer {
        public override void Serialize(BsonWriter bsonWriter,
            Type nominalType, object value, IBsonSerializationOptions options) {
            AppBrief brief = value as AppBrief;
            bsonWriter.WriteInt32(brief.Id);
        }

        public override object Deserialize(BsonReader bsonReader,
            Type nominalType, Type actualType, IBsonSerializationOptions options) {
            if (nominalType != typeof(AppBrief) || actualType != typeof(AppBrief)) {
                throw new BsonSerializationException("This serializer can only serialize type PingApp.Entity.AppBrief");
            }

            if (bsonReader.GetCurrentBsonType() != BsonType.Int32) {
                throw new FormatException("AppBrief should be serialized to Int32");
            }

            int id = bsonReader.ReadInt32();
            return new AppBrief() { Id = id };
        }
    }
}
