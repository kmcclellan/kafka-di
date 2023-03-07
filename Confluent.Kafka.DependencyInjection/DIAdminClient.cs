namespace Confluent.Kafka.DependencyInjection;

using Confluent.Kafka.Admin;

using Microsoft.Extensions.DependencyInjection;

sealed class DIAdminClient : IAdminClient
{
    readonly IServiceScope scope;
    readonly IAdminClient inner;

    public DIAdminClient(IServiceScopeFactory factory)
    {
        this.scope = factory.CreateScope();
        this.inner = this.scope.ServiceProvider.GetRequiredService<KafkaBuilderFactory>().CreateAdmin().Build();
    }

    public Handle Handle => this.inner.Handle;

    public string Name => this.inner.Name;

    public int AddBrokers(string brokers)
    {
        return this.inner.AddBrokers(brokers);
    }

    public Metadata GetMetadata(TimeSpan timeout)
    {
        return this.inner.GetMetadata(timeout);
    }

    public Metadata GetMetadata(string topic, TimeSpan timeout)
    {
        return this.inner.GetMetadata(topic, timeout);
    }

    public Task CreateTopicsAsync(IEnumerable<TopicSpecification> topics, CreateTopicsOptions? options = null)
    {
        return this.inner.CreateTopicsAsync(topics, options);
    }

    public Task DeleteTopicsAsync(IEnumerable<string> topics, DeleteTopicsOptions? options = null)
    {
        return this.inner.DeleteTopicsAsync(topics, options);
    }

    public Task CreatePartitionsAsync(
        IEnumerable<PartitionsSpecification> partitionsSpecifications,
        CreatePartitionsOptions? options = null)
    {
        return this.inner.CreatePartitionsAsync(partitionsSpecifications, options);
    }

    public Task<List<DeleteRecordsResult>> DeleteRecordsAsync(
        IEnumerable<TopicPartitionOffset> topicPartitionOffsets,
        DeleteRecordsOptions? options = null)
    {
        return this.inner.DeleteRecordsAsync(topicPartitionOffsets, options);
    }

    public GroupInfo ListGroup(string group, TimeSpan timeout)
    {
        return this.inner.ListGroup(group, timeout);
    }

    public List<GroupInfo> ListGroups(TimeSpan timeout)
    {
        return this.inner.ListGroups(timeout);
    }

    public Task<ListConsumerGroupsResult> ListConsumerGroupsAsync(ListConsumerGroupsOptions? options = null)
    {
        return this.inner.ListConsumerGroupsAsync(options);
    }

    public Task<DescribeConsumerGroupsResult> DescribeConsumerGroupsAsync(
        IEnumerable<string> groups,
        DescribeConsumerGroupsOptions? options = null)
    {
        return this.inner.DescribeConsumerGroupsAsync(groups, options);
    }

    public Task DeleteGroupsAsync(IList<string> groups, DeleteGroupsOptions? options = null)
    {
        return this.inner.DeleteGroupsAsync(groups, options);
    }

    public Task<List<ListConsumerGroupOffsetsResult>> ListConsumerGroupOffsetsAsync(
        IEnumerable<ConsumerGroupTopicPartitions> groupPartitions,
        ListConsumerGroupOffsetsOptions? options = null)
    {
        return this.inner.ListConsumerGroupOffsetsAsync(groupPartitions, options);
    }

    public Task<List<AlterConsumerGroupOffsetsResult>> AlterConsumerGroupOffsetsAsync(
        IEnumerable<ConsumerGroupTopicPartitionOffsets> groupPartitions,
        AlterConsumerGroupOffsetsOptions? options = null)
    {
        return this.inner.AlterConsumerGroupOffsetsAsync(groupPartitions, options);
    }

    public Task<DeleteConsumerGroupOffsetsResult> DeleteConsumerGroupOffsetsAsync(
        string group,
        IEnumerable<TopicPartition> partitions,
        DeleteConsumerGroupOffsetsOptions? options = null)
    {
        return this.inner.DeleteConsumerGroupOffsetsAsync(group, partitions, options);
    }

    public Task<List<DescribeConfigsResult>> DescribeConfigsAsync(
        IEnumerable<ConfigResource> resources,
        DescribeConfigsOptions? options = null)
    {
        return this.inner.DescribeConfigsAsync(resources, options);
    }

    public Task AlterConfigsAsync(
        Dictionary<ConfigResource, List<ConfigEntry>> configs,
        AlterConfigsOptions? options = null)
    {
        return this.inner.AlterConfigsAsync(configs, options);
    }

    public Task<DescribeAclsResult> DescribeAclsAsync(
        AclBindingFilter aclBindingFilter,
        DescribeAclsOptions? options = null)
    {
        return this.inner.DescribeAclsAsync(aclBindingFilter, options);
    }

    public Task CreateAclsAsync(IEnumerable<AclBinding> aclBindings, CreateAclsOptions? options = null)
    {
        return this.inner.CreateAclsAsync(aclBindings, options);
    }

    public Task<List<DeleteAclsResult>> DeleteAclsAsync(
        IEnumerable<AclBindingFilter> aclBindingFilters,
        DeleteAclsOptions? options = null)
    {
        return this.inner.DeleteAclsAsync(aclBindingFilters, options);
    }

    public void Dispose()
    {
        this.inner.Dispose();
        this.scope.Dispose();
    }
}
