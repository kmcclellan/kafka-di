namespace Confluent.Kafka.DependencyInjection.Tests;

using Confluent.Kafka;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Linq;

[TestClass]
public sealed class MSConfigurationExtensionsTests
{
    const string BootstrapServers = "1.2.3.4:1000,4.5.6.7:1001";

    readonly Dictionary<string, string> appConfig = new()
    {
        { "bootstrap.servers", BootstrapServers },
        { "acks", "-1" },
    };

    readonly List<string> sectionKeys = [];

    [TestMethod]
    public void LoadsAdminClientProperties()
    {
        var clientConfig = new AdminClientConfig();
        clientConfig.LoadFrom(GetConfiguration());

        VerifySection("Admin");
        VerifySharedProperties(clientConfig);
    }

    [TestMethod]
    public void LoadsConsumerProperties()
    {
        var groupId = "gamma-771";
        appConfig.Add("group.id", groupId);
        appConfig.Add("partition.assignment.strategy", "cooperative-sticky");

        var clientConfig = new ConsumerConfig();
        clientConfig.LoadFrom(GetConfiguration());

        VerifySection("Consumer");
        VerifySharedProperties(clientConfig);

        Assert.AreEqual(groupId, clientConfig.GroupId);
        Assert.AreEqual(PartitionAssignmentStrategy.CooperativeSticky, clientConfig.PartitionAssignmentStrategy);
    }

    [TestMethod]
    public void LoadsProducerProperties()
    {
        var transactionId = "epsilon-319";
        appConfig.Add("transactional.id", transactionId);
        appConfig.Add("partitioner", "murmur2");

        var clientConfig = new ProducerConfig();
        clientConfig.LoadFrom(GetConfiguration());

        VerifySection("Producer");
        VerifySharedProperties(clientConfig);

        Assert.AreEqual(transactionId, clientConfig.TransactionalId);
        Assert.AreEqual(Partitioner.Murmur2, clientConfig.Partitioner);
    }

    private ConfigurationBranch GetConfiguration()
    {
        var children = appConfig.Select(x => new ConfigurationLeaf(x.Key) { Value = x.Value });
        return new ConfigurationBranch(sectionKeys, children);
    }

    private void VerifySection(string name)
    {
        CollectionAssert.AreEqual(
            sectionKeys,
            new[] { "Kafka", name },
            $"Unexpected section path: {string.Join(':', sectionKeys)} ");
    }

    private static void VerifySharedProperties(ClientConfig clientConfig)
    {
        Assert.AreEqual(BootstrapServers, clientConfig.BootstrapServers);
        Assert.AreEqual(Acks.All, clientConfig.Acks);
    }

    sealed class ConfigurationBranch(
        List<string> keys,
        IEnumerable<IConfigurationSection> children) :
        IConfigurationSection
    {
        public string Path => throw new NotImplementedException();

        public string Key => throw new NotImplementedException();

        public string? Value
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public string? this[string key]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public IConfigurationSection GetSection(string key)
        {
            keys.Add(key);
            return this;
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return children;
        }

        public IChangeToken GetReloadToken()
        {
            throw new NotImplementedException();
        }
    }

    sealed class ConfigurationLeaf(string key) : IConfigurationSection
    {
        public string Path => throw new NotImplementedException();

        public string Key => key;

        public string? Value { get; set; }

        public string? this[string key]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public IConfigurationSection GetSection(string key)
        {
            return new ConfigurationLeaf(key);
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return [];
        }

        public IChangeToken GetReloadToken()
        {
            throw new NotImplementedException();
        }
    }
}
