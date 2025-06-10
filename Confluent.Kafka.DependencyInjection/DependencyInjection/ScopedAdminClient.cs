namespace Confluent.Kafka.DependencyInjection;

using Confluent.Kafka.Admin;
using Confluent.Kafka.Options;

using Microsoft.Extensions.DependencyInjection;

sealed class ScopedAdminClient : IAdminClient
{
    readonly IServiceScope scope;
    readonly IAdminClient client;

    public ScopedAdminClient(IServiceScopeFactory scopes)
    {
        scope = scopes.CreateScope();

        var config = new Dictionary<string, string>();

        foreach (var provider in scope.ServiceProvider.GetServices<IClientConfigProvider>())
        {
            var iterator = provider.ForAdminClient();

            while (iterator.MoveNext())
            {
                config[iterator.Current.Key] = iterator.Current.Value;
            }
        }

        var builder = new AdminClientBuilder(config);

        foreach (var setup in scope.ServiceProvider.GetServices<IClientBuilderSetup>())
        {
            setup.Apply(builder);
        }

        client = builder.Build();
    }

    public Handle Handle => client.Handle;

    public string Name => client.Name;

    public int AddBrokers(string brokers)
    {
        return client.AddBrokers(brokers);
    }

    public void SetSaslCredentials(string username, string password)
    {
        client.SetSaslCredentials(username, password);
    }

    public Task<DescribeUserScramCredentialsResult> DescribeUserScramCredentialsAsync(
        IEnumerable<string> users,
        DescribeUserScramCredentialsOptions? options = null)
    {
        return client.DescribeUserScramCredentialsAsync(users, options);
    }

    public Task AlterUserScramCredentialsAsync(
        IEnumerable<UserScramCredentialAlteration> alterations,
        AlterUserScramCredentialsOptions? options = null)
    {
        return client.AlterUserScramCredentialsAsync(alterations, options);
    }

    public Metadata GetMetadata(TimeSpan timeout)
    {
        return client.GetMetadata(timeout);
    }

    public Metadata GetMetadata(string topic, TimeSpan timeout)
    {
        return client.GetMetadata(topic, timeout);
    }

    public Task CreateTopicsAsync(IEnumerable<TopicSpecification> topics, CreateTopicsOptions? options = null)
    {
        return client.CreateTopicsAsync(topics, options);
    }

    public Task DeleteTopicsAsync(IEnumerable<string> topics, DeleteTopicsOptions? options = null)
    {
        return client.DeleteTopicsAsync(topics, options);
    }

    public Task CreatePartitionsAsync(
        IEnumerable<PartitionsSpecification> partitionsSpecifications,
        CreatePartitionsOptions? options = null)
    {
        return client.CreatePartitionsAsync(partitionsSpecifications, options);
    }

    public Task<List<DeleteRecordsResult>> DeleteRecordsAsync(
        IEnumerable<TopicPartitionOffset> topicPartitionOffsets,
        DeleteRecordsOptions? options = null)
    {
        return client.DeleteRecordsAsync(topicPartitionOffsets, options);
    }

    public GroupInfo ListGroup(string group, TimeSpan timeout)
    {
        return client.ListGroup(group, timeout);
    }

    public List<GroupInfo> ListGroups(TimeSpan timeout)
    {
        return client.ListGroups(timeout);
    }

    public Task<ListConsumerGroupsResult> ListConsumerGroupsAsync(ListConsumerGroupsOptions? options = null)
    {
        return client.ListConsumerGroupsAsync(options);
    }

    public Task<DescribeConsumerGroupsResult> DescribeConsumerGroupsAsync(
        IEnumerable<string> groups,
        DescribeConsumerGroupsOptions? options = null)
    {
        return client.DescribeConsumerGroupsAsync(groups, options);
    }

    public Task DeleteGroupsAsync(IList<string> groups, DeleteGroupsOptions? options = null)
    {
        return client.DeleteGroupsAsync(groups, options);
    }

    public Task<List<ListConsumerGroupOffsetsResult>> ListConsumerGroupOffsetsAsync(
        IEnumerable<ConsumerGroupTopicPartitions> groupPartitions,
        ListConsumerGroupOffsetsOptions? options = null)
    {
        return client.ListConsumerGroupOffsetsAsync(groupPartitions, options);
    }

    public Task<List<AlterConsumerGroupOffsetsResult>> AlterConsumerGroupOffsetsAsync(
        IEnumerable<ConsumerGroupTopicPartitionOffsets> groupPartitions,
        AlterConsumerGroupOffsetsOptions? options = null)
    {
        return client.AlterConsumerGroupOffsetsAsync(groupPartitions, options);
    }

    public Task<DeleteConsumerGroupOffsetsResult> DeleteConsumerGroupOffsetsAsync(
        string group,
        IEnumerable<TopicPartition> partitions,
        DeleteConsumerGroupOffsetsOptions? options = null)
    {
        return client.DeleteConsumerGroupOffsetsAsync(group, partitions, options);
    }

    public Task<List<DescribeConfigsResult>> DescribeConfigsAsync(
        IEnumerable<ConfigResource> resources,
        DescribeConfigsOptions? options = null)
    {
        return client.DescribeConfigsAsync(resources, options);
    }

    public Task AlterConfigsAsync(
        Dictionary<ConfigResource, List<ConfigEntry>> configs,
        AlterConfigsOptions? options = null)
    {
        return client.AlterConfigsAsync(configs, options);
    }

    public Task<List<IncrementalAlterConfigsResult>> IncrementalAlterConfigsAsync(
        Dictionary<ConfigResource, List<ConfigEntry>> configs,
        IncrementalAlterConfigsOptions? options = null)
    {
        return client.IncrementalAlterConfigsAsync(configs, options);
    }

    public Task<DescribeAclsResult> DescribeAclsAsync(
        AclBindingFilter aclBindingFilter,
        DescribeAclsOptions? options = null)
    {
        return client.DescribeAclsAsync(aclBindingFilter, options);
    }

    public Task CreateAclsAsync(IEnumerable<AclBinding> aclBindings, CreateAclsOptions? options = null)
    {
        return client.CreateAclsAsync(aclBindings, options);
    }

    public Task<List<DeleteAclsResult>> DeleteAclsAsync(
        IEnumerable<AclBindingFilter> aclBindingFilters,
        DeleteAclsOptions? options = null)
    {
        return client.DeleteAclsAsync(aclBindingFilters, options);
    }

    public void Dispose()
    {
        client.Dispose();
        scope.Dispose();
    }
}
