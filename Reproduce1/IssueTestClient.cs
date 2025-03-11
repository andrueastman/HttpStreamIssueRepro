using Microsoft.Kiota.Abstractions;
using TestClient;

namespace Reproduce1;

public class IssueTestClient(IRequestAdapter requestAdapter) : IssueTestKiotaClient(requestAdapter), IIssueTestClient;