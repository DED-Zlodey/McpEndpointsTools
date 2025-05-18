using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace McpEndpointsTools.Infrastructure;

/// <summary>
/// Represents a specialized implementation of the `DelegatingMcpServerTool` that combines
/// functionality from two different `McpServerTool` instances.
/// </summary>
/// <remarks>
/// The `McpServerToolWithDifferentImpl` provides a mechanism to delegate core operations
/// to a base implementation while also exposing an additional tool for protocol-specific functionality.
/// This class is immutable and thread-safe as there are no modifications to internal state after construction.
/// </remarks>
/// <example>
/// This class is typically used in systems where tools need to be dynamically assembled
/// for route registration or endpoint infrastructure. It allows combining tools for
/// universal handling while allowing specific protocol-based behavior to be encapsulated.
/// </example>
/// <param name="impl">
/// The primary `McpServerTool` instance that serves as the base for delegating operations.
/// </param>
/// <param name="protocol">
/// A secondary `McpServerTool` instance that provides specialized protocol-related functionality.
/// </param>
/// <property name="ProtocolTool">
/// Exposes the `Tool` encapsulated within the secondary protocol-related `McpServerTool` instance.
/// </property>
internal sealed class McpServerToolWithDifferentImpl : DelegatingMcpServerTool
{
    /// Represents the protocol-specific tool that is exposed within the
    /// `McpServerToolWithDifferentImpl` class. This property provides access
    /// to functionality tied to the protocol-based instance of `McpServerTool`,
    /// allowing for operations that rely on protocol-specific behavior.
    /// This property is immutable and derived from the secondary `McpServerTool`
    /// instance, ensuring thread safety and consistency across usages.
    /// Use cases involve scenarios requiring integration of protocol-based tools
    /// into systems with dynamically composed server tool functionalities.
    public override Tool ProtocolTool { get; }

    /// Represents a specialized implementation of the `DelegatingMcpServerTool` that combines
    /// functionality from two different instances of `McpServerTool`.
    /// The primary purpose is to delegate operations to a provided base implementation and
    /// to expose a secondary protocol-related tool for additional functionality.
    /// This class is immutable and thread-safe as no internal state is modified after construction.
    /// Constructor Parameters:
    /// - `impl`: The primary instance of `McpServerTool` to delegate operations to.
    /// - `protocol`: The secondary `McpServerTool` instance that provides protocol-specific functionality.
    /// Properties:
    /// - `ProtocolTool`: Exposes the protocol-based `Tool` instance encapsulated within the `protocol` object.
    /// Example Use Case:
    /// This class is typically utilized in systems where tools are dynamically assembled and used
    /// in conjunction with route registrations or endpoint handling infrastructure.
    public McpServerToolWithDifferentImpl(McpServerTool impl, McpServerTool protocol)
        : base(impl)
    {
        ProtocolTool = protocol.ProtocolTool;
    }
}