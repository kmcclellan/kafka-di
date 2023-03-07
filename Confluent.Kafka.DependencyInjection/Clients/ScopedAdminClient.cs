namespace Confluent.Kafka.DependencyInjection.Clients;

using Confluent.Kafka.Admin;

using Microsoft.Extensions.DependencyInjection;

class ScopedAdminClient : IAdminClient
{
    readonly IAdminClient client;
    readonly IDisposable scope;

    public ScopedAdminClient(IServiceScopeFactory scopes, IEnumerable<KeyValuePair<string, string>>? config)
    {
        IServiceScope scope;
        this.scope = scope = scopes.CreateScope();

        if (config != null)
        {
            var merged = scope.ServiceProvider.GetRequiredService<AdminClientConfig>();

            foreach (var kvp in config)
            {
                merged.Set(kvp.Key, kvp.Value);
            }
        }

        this.client = scope.ServiceProvider.GetRequiredService<AdminClientBuilder>().Build();
    }

    public Handle Handle => this.client.Handle;

    public string Name => this.client.Name;

    public int AddBrokers(string brokers)
    {
        return this.client.AddBrokers(brokers);
    }

    public Metadata GetMetadata(TimeSpan timeout)
    {
        return this.client.GetMetadata(timeout);
    }

    public Metadata GetMetadata(string topic, TimeSpan timeout)
    {
        return this.client.GetMetadata(topic, timeout);
    }

    public Task CreateTopicsAsync(IEnumerable<TopicSpecification> topics, CreateTopicsOptions? options = null)
    {
        return this.client.CreateTopicsAsync(topics, options);
    }

    public Task DeleteTopicsAsync(IEnumerable<string> topics, DeleteTopicsOptions? options = null)
    {
        return this.client.DeleteTopicsAsync(topics, options);
    }

    public Task CreatePartitionsAsync(
        IEnumerable<PartitionsSpecification> partitionsSpecifications,
        CreatePartitionsOptions? options = null)
    {
        return this.client.CreatePartitionsAsync(partitionsSpecifications, options);
    }

    public Task<List<DeleteRecordsResult>> DeleteRecordsAsync(
        IEnumerable<TopicPartitionOffset> topicPartitionOffsets,
        DeleteRecordsOptions? options = null)
    {
        return this.client.DeleteRecordsAsync(topicPartitionOffsets, options);
    }

    public GroupInfo ListGroup(string group, TimeSpan timeout)
    {
        return this.client.ListGroup(group, timeout);
    }

    public List<GroupInfo> ListGroups(TimeSpan timeout)
    {
        return this.client.ListGroups(timeout);
    }

    public Task<ListConsumerGroupsResult> ListConsumerGroupsAsync(ListConsumerGroupsOptions? options = null)
    {
        return this.client.ListConsumerGroupsAsync(options);
    }

    public Task<DescribeConsumerGroupsResult> DescribeConsumerGroupsAsync(
        IEnumerable<string> groups,
        DescribeConsumerGroupsOptions? options = null)
    {
        return this.client.DescribeConsumerGroupsAsync(groups, options);
    }

    public Task DeleteGroupsAsync(IList<string> groups, DeleteGroupsOptions? options = null)
    {
        return this.client.DeleteGroupsAsync(groups, options);
    }

    public Task<List<ListConsumerGroupOffsetsResult>> ListConsumerGroupOffsetsAsync(
        IEnumerable<ConsumerGroupTopicPartitions> groupPartitions,
        ListConsumerGroupOffsetsOptions? options = null)
    {
        return this.client.ListConsumerGroupOffsetsAsync(groupPartitions, options);
    }

    public Task<List<AlterConsumerGroupOffsetsResult>> AlterConsumerGroupOffsetsAsync(
        IEnumerable<ConsumerGroupTopicPartitionOffsets> groupPartitions,
        AlterConsumerGroupOffsetsOptions? options = null)
    {
        return this.client.AlterConsumerGroupOffsetsAsync(groupPartitions, options);
    }

    public Task<DeleteConsumerGroupOffsetsResult> DeleteConsumerGroupOffsetsAsync(
        string group,
        IEnumerable<TopicPartition> partitions,
        DeleteConsumerGroupOffsetsOptions? options = null)
    {
        return this.client.DeleteConsumerGroupOffsetsAsync(group, partitions, options);
    }

    public Task<List<DescribeConfigsResult>> DescribeConfigsAsync(
        IEnumerable<ConfigResource> resources,
        DescribeConfigsOptions? options = null)
    {
        return this.client.DescribeConfigsAsync(resources, options);
    }

    public Task AlterConfigsAsync(
        Dictionary<ConfigResource, List<ConfigEntry>> configs,
        AlterConfigsOptions? options = null)
    {
        return this.client.AlterConfigsAsync(configs, options);
    }

    public Task<DescribeAclsResult> DescribeAclsAsync(
        AclBindingFilter aclBindingFilter,
        DescribeAclsOptions? options = null)
    {
        return this.client.DescribeAclsAsync(aclBindingFilter, options);
    }

    public Task CreateAclsAsync(IEnumerable<AclBinding> aclBindings, CreateAclsOptions? options = null)
    {
        return this.client.CreateAclsAsync(aclBindings, options);
    }

    public Task<List<DeleteAclsResult>> DeleteAclsAsync(
        IEnumerable<AclBindingFilter> aclBindingFilters,
        DeleteAclsOptions? options = null)
    {
        return this.client.DeleteAclsAsync(aclBindingFilters, options);
    }

    public void Dispose()
    {
        this.client.Dispose();
        this.scope.Dispose();
    }
}
