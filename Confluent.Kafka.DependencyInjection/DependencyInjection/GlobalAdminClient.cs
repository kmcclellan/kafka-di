namespace Confluent.Kafka.DependencyInjection
{
    using Confluent.Kafka.Admin;

    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    sealed class GlobalAdminClient : IAdminClient
    {
        readonly IEnumerable<IClientConfigProvider> configProviders;
        readonly IEnumerable<IClientBuilderSetup> builderSetups;
        readonly object syncObj = new object();

        IAdminClient client;

        public GlobalAdminClient(
            IEnumerable<IClientConfigProvider> configProviders,
            IEnumerable<IClientBuilderSetup> builderSetups)
        {
            this.configProviders = configProviders;
            this.builderSetups = builderSetups;
        }

        public Handle Handle => Client.Handle;

        public string Name => Client.Name;

        IAdminClient Client
        {
            get
            {
                if (client == null)
                {
                    lock (syncObj)
                    {
                        if (client == null)
                        {
                            var config = new Dictionary<string, string>();

                            foreach (var provider in configProviders)
                            {
                                var iterator = provider.ForAdminClient();

                                while (iterator.MoveNext())
                                {
                                    config[iterator.Current.Key] = iterator.Current.Value;
                                }
                            }

                            var builder = new AdminClientBuilder(config);

                            foreach (var setup in builderSetups)
                            {
                                setup.Apply(builder);
                            }

                            client = builder.Build();
                        }
                    }
                }

                return client;
            }
        }

        public int AddBrokers(string brokers)
        {
            return Client.AddBrokers(brokers);
        }

        public void SetSaslCredentials(string username, string password)
        {
            Client.SetSaslCredentials(username, password);
        }

        public Task<DescribeUserScramCredentialsResult> DescribeUserScramCredentialsAsync(
            IEnumerable<string> users,
            DescribeUserScramCredentialsOptions options = null)
        {
            return Client.DescribeUserScramCredentialsAsync(users, options);
        }

        public Task AlterUserScramCredentialsAsync(
            IEnumerable<UserScramCredentialAlteration> alterations,
            AlterUserScramCredentialsOptions options = null)
        {
            return Client.AlterUserScramCredentialsAsync(alterations, options);
        }

        public Metadata GetMetadata(TimeSpan timeout)
        {
            return Client.GetMetadata(timeout);
        }

        public Metadata GetMetadata(string topic, TimeSpan timeout)
        {
            return Client.GetMetadata(topic, timeout);
        }

        public Task CreateTopicsAsync(IEnumerable<TopicSpecification> topics, CreateTopicsOptions options = null)
        {
            return Client.CreateTopicsAsync(topics, options);
        }

        public Task DeleteTopicsAsync(IEnumerable<string> topics, DeleteTopicsOptions options = null)
        {
            return Client.DeleteTopicsAsync(topics, options);
        }

        public Task CreatePartitionsAsync(
            IEnumerable<PartitionsSpecification> partitionsSpecifications,
            CreatePartitionsOptions options = null)
        {
            return Client.CreatePartitionsAsync(partitionsSpecifications, options);
        }

        public Task<List<DeleteRecordsResult>> DeleteRecordsAsync(
            IEnumerable<TopicPartitionOffset> topicPartitionOffsets,
            DeleteRecordsOptions options = null)
        {
            return Client.DeleteRecordsAsync(topicPartitionOffsets, options);
        }

        public GroupInfo ListGroup(string group, TimeSpan timeout)
        {
            return Client.ListGroup(group, timeout);
        }

        public List<GroupInfo> ListGroups(TimeSpan timeout)
        {
            return Client.ListGroups(timeout);
        }

        public Task<ListConsumerGroupsResult> ListConsumerGroupsAsync(ListConsumerGroupsOptions options = null)
        {
            return Client.ListConsumerGroupsAsync(options);
        }

        public Task<DescribeConsumerGroupsResult> DescribeConsumerGroupsAsync(
            IEnumerable<string> groups,
            DescribeConsumerGroupsOptions options = null)
        {
            return Client.DescribeConsumerGroupsAsync(groups, options);
        }

        public Task DeleteGroupsAsync(IList<string> groups, DeleteGroupsOptions options = null)
        {
            return Client.DeleteGroupsAsync(groups, options);
        }

        public Task<List<ListConsumerGroupOffsetsResult>> ListConsumerGroupOffsetsAsync(
            IEnumerable<ConsumerGroupTopicPartitions> groupPartitions,
            ListConsumerGroupOffsetsOptions options = null)
        {
            return Client.ListConsumerGroupOffsetsAsync(groupPartitions, options);
        }

        public Task<List<AlterConsumerGroupOffsetsResult>> AlterConsumerGroupOffsetsAsync(
            IEnumerable<ConsumerGroupTopicPartitionOffsets> groupPartitions,
            AlterConsumerGroupOffsetsOptions options = null)
        {
            return Client.AlterConsumerGroupOffsetsAsync(groupPartitions, options);
        }

        public Task<DeleteConsumerGroupOffsetsResult> DeleteConsumerGroupOffsetsAsync(
            string group,
            IEnumerable<TopicPartition> partitions,
            DeleteConsumerGroupOffsetsOptions options = null)
        {
            return Client.DeleteConsumerGroupOffsetsAsync(group, partitions, options);
        }

        public Task<List<DescribeConfigsResult>> DescribeConfigsAsync(
            IEnumerable<ConfigResource> resources,
            DescribeConfigsOptions options = null)
        {
            return Client.DescribeConfigsAsync(resources, options);
        }

        public Task AlterConfigsAsync(
            Dictionary<ConfigResource, List<ConfigEntry>> configs,
            AlterConfigsOptions options = null)
        {
            return Client.AlterConfigsAsync(configs, options);
        }

        public Task<List<IncrementalAlterConfigsResult>> IncrementalAlterConfigsAsync(
            Dictionary<ConfigResource, List<ConfigEntry>> configs,
            IncrementalAlterConfigsOptions options = null)
        {
            return Client.IncrementalAlterConfigsAsync(configs, options);
        }

        public Task<DescribeAclsResult> DescribeAclsAsync(
            AclBindingFilter aclBindingFilter,
            DescribeAclsOptions options = null)
        {
            return Client.DescribeAclsAsync(aclBindingFilter, options);
        }

        public Task CreateAclsAsync(IEnumerable<AclBinding> aclBindings, CreateAclsOptions options = null)
        {
            return Client.CreateAclsAsync(aclBindings, options);
        }

        public Task<List<DeleteAclsResult>> DeleteAclsAsync(
            IEnumerable<AclBindingFilter> aclBindingFilters,
            DeleteAclsOptions options = null)
        {
            return Client.DeleteAclsAsync(aclBindingFilters, options);
        }

        public void Dispose()
        {
            client?.Dispose();
        }
    }
}
