// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: file_storage.proto
// </auto-generated>
#pragma warning disable 0414, 1591
#region Designer generated code

using grpc = global::Grpc.Core;

namespace KP_Server {
  public static partial class FileStorage
  {
    static readonly string __ServiceName = "standard.FileStorage";

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static void __Helper_SerializeMessage(global::Google.Protobuf.IMessage message, grpc::SerializationContext context)
    {
      #if !GRPC_DISABLE_PROTOBUF_BUFFER_SERIALIZATION
      if (message is global::Google.Protobuf.IBufferMessage)
      {
        context.SetPayloadLength(message.CalculateSize());
        global::Google.Protobuf.MessageExtensions.WriteTo(message, context.GetBufferWriter());
        context.Complete();
        return;
      }
      #endif
      context.Complete(global::Google.Protobuf.MessageExtensions.ToByteArray(message));
    }

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static class __Helper_MessageCache<T>
    {
      public static readonly bool IsBufferMessage = global::System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(global::Google.Protobuf.IBufferMessage)).IsAssignableFrom(typeof(T));
    }

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static T __Helper_DeserializeMessage<T>(grpc::DeserializationContext context, global::Google.Protobuf.MessageParser<T> parser) where T : global::Google.Protobuf.IMessage<T>
    {
      #if !GRPC_DISABLE_PROTOBUF_BUFFER_SERIALIZATION
      if (__Helper_MessageCache<T>.IsBufferMessage)
      {
        return parser.ParseFrom(context.PayloadAsReadOnlySequence());
      }
      #endif
      return parser.ParseFrom(context.PayloadAsNewBuffer());
    }

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::KP_Server.FileStorageUploadRequest> __Marshaller_standard_FileStorageUploadRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::KP_Server.FileStorageUploadRequest.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::KP_Server.FileStorageUploadReply> __Marshaller_standard_FileStorageUploadReply = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::KP_Server.FileStorageUploadReply.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::KP_Server.FileStorageDownloadRequest> __Marshaller_standard_FileStorageDownloadRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::KP_Server.FileStorageDownloadRequest.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::KP_Server.FileStorageDownloadReply> __Marshaller_standard_FileStorageDownloadReply = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::KP_Server.FileStorageDownloadReply.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::KP_Server.FileStorageRemoveRequest> __Marshaller_standard_FileStorageRemoveRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::KP_Server.FileStorageRemoveRequest.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::KP_Server.FileStorageRemoveReply> __Marshaller_standard_FileStorageRemoveReply = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::KP_Server.FileStorageRemoveReply.Parser));

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::KP_Server.FileStorageUploadRequest, global::KP_Server.FileStorageUploadReply> __Method_upload = new grpc::Method<global::KP_Server.FileStorageUploadRequest, global::KP_Server.FileStorageUploadReply>(
        grpc::MethodType.Unary,
        __ServiceName,
        "upload",
        __Marshaller_standard_FileStorageUploadRequest,
        __Marshaller_standard_FileStorageUploadReply);

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::KP_Server.FileStorageDownloadRequest, global::KP_Server.FileStorageDownloadReply> __Method_download = new grpc::Method<global::KP_Server.FileStorageDownloadRequest, global::KP_Server.FileStorageDownloadReply>(
        grpc::MethodType.Unary,
        __ServiceName,
        "download",
        __Marshaller_standard_FileStorageDownloadRequest,
        __Marshaller_standard_FileStorageDownloadReply);

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::KP_Server.FileStorageRemoveRequest, global::KP_Server.FileStorageRemoveReply> __Method_remove = new grpc::Method<global::KP_Server.FileStorageRemoveRequest, global::KP_Server.FileStorageRemoveReply>(
        grpc::MethodType.Unary,
        __ServiceName,
        "remove",
        __Marshaller_standard_FileStorageRemoveRequest,
        __Marshaller_standard_FileStorageRemoveReply);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::KP_Server.FileStorageReflection.Descriptor.Services[0]; }
    }

    /// <summary>Base class for server-side implementations of FileStorage</summary>
    [grpc::BindServiceMethod(typeof(FileStorage), "BindService")]
    public abstract partial class FileStorageBase
    {
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task<global::KP_Server.FileStorageUploadReply> upload(global::KP_Server.FileStorageUploadRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task<global::KP_Server.FileStorageDownloadReply> download(global::KP_Server.FileStorageDownloadRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task<global::KP_Server.FileStorageRemoveReply> remove(global::KP_Server.FileStorageRemoveRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

    }

    /// <summary>Creates service definition that can be registered with a server</summary>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    public static grpc::ServerServiceDefinition BindService(FileStorageBase serviceImpl)
    {
      return grpc::ServerServiceDefinition.CreateBuilder()
          .AddMethod(__Method_upload, serviceImpl.upload)
          .AddMethod(__Method_download, serviceImpl.download)
          .AddMethod(__Method_remove, serviceImpl.remove).Build();
    }

    /// <summary>Register service method with a service binder with or without implementation. Useful when customizing the  service binding logic.
    /// Note: this method is part of an experimental API that can change or be removed without any prior notice.</summary>
    /// <param name="serviceBinder">Service methods will be bound by calling <c>AddMethod</c> on this object.</param>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    public static void BindService(grpc::ServiceBinderBase serviceBinder, FileStorageBase serviceImpl)
    {
      serviceBinder.AddMethod(__Method_upload, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::KP_Server.FileStorageUploadRequest, global::KP_Server.FileStorageUploadReply>(serviceImpl.upload));
      serviceBinder.AddMethod(__Method_download, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::KP_Server.FileStorageDownloadRequest, global::KP_Server.FileStorageDownloadReply>(serviceImpl.download));
      serviceBinder.AddMethod(__Method_remove, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::KP_Server.FileStorageRemoveRequest, global::KP_Server.FileStorageRemoveReply>(serviceImpl.remove));
    }

  }
}
#endregion
