using FluentResults;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Ingweland.Fog.Inn.Models.Hoh.Errors;

namespace Ingweland.Fog.Inn.Models.Hoh.Extensions;

public static class AnyHelper
{
    public static T FindAndUnpack<T>(this RepeatedField<Any> items) where T : IMessage<T>, new()
    {
        var message = new T();
        var unpacked = items
            .Where(item => Any.GetTypeName(item.TypeUrl) == message.Descriptor.Name)
            .Select(item => item.Unpack<T>())
            .ToList();
        if (unpacked.Count != 1)
        {
            throw new InvalidOperationException($"Expected exactly one instance of {message.Descriptor.Name
            }, but found {unpacked.Count}.");
        }

        return unpacked[0];
    }

    public static IList<T> FindAndUnpackToList<T>(this RepeatedField<Any> items) where T : IMessage<T>, new()
    {
        var message = new T();
        return items
            .Where(item => Any.GetTypeName(item.TypeUrl) == message.Descriptor.Name)
            .Select(item => item.Unpack<T>())
            .ToList();
    }

    public static IList<T> FindAndUnpackToList<T>(this RepeatedField<Any> items, string typeName)
        where T : IMessage<T>, new()
    {
        return items
            .Where(item => Any.GetTypeName(item.TypeUrl) == typeName)
            .Select(item =>
            {
                var message = new T();
                message.MergeFrom(item.Value);
                return message;
            })
            .ToList();
    }

    public static Result<T> FindAndUnpackToResult<T>(this RepeatedField<Any> items) where T : IMessage<T>, new()
    {
        var message = new T();
        List<T> unpacked;
        try
        {
            unpacked = items
                .Where(item => Any.GetTypeName(item.TypeUrl) == message.Descriptor.Name)
                .Select(item => item.Unpack<T>())
                .ToList();
        }
        catch (Exception e)
        {
            return Result.Fail<T>(new HohProtobufParsingError(ProtobufParsingStage.AnyMessageUnpacking, nameof(T), e));
        }

        if (unpacked.Count != 1)
        {
            return Result.Fail<T>(new HohInvalidCardinalityError(
                $"Expected exactly one instance of {message.Descriptor.Name}, but found {unpacked.Count}."));
        }

        return Result.Ok(unpacked[0]);
    }

    public static bool Contains<T>(this RepeatedField<Any> items) where T : IMessage<T>, new()
    {
        return items.Any(item => Any.GetTypeName(item.TypeUrl) == new T().Descriptor.Name);
    }
}
