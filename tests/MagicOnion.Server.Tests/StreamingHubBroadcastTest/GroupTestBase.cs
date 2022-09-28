using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using Xunit;

namespace MagicOnion.Server.Tests.StreamingHubBroadcastTest
{
    public abstract class GroupTestBase
    {
        readonly ServerFixture server;

        public GroupTestBase(ServerFixture server)
        {
            this.server = server;
        }

        [Fact]
        public async Task BroadcastToSelf()
        {
            var mockReceiver = new StreamingHubBroadcastTestHubReceiverMock();
            var hub = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiver);
            await hub.RegisterConnectionToGroup();

            var mockReceiverOther = new StreamingHubBroadcastTestHubReceiverMock();
            var hubOther = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiverOther);
            await hubOther.RegisterConnectionToGroup();

            Assert.False(mockReceiver.HasCalled);
            Assert.False(mockReceiverOther.HasCalled);

            await hub.CallBroadcastToSelfAsync();

            await Task.Delay(10); // NOTE: The receivers may not receive broadcast yet at this point.

            // Target: Self
            Assert.True(mockReceiver.HasCalled);
            Assert.False(mockReceiverOther.HasCalled);
        }

        [Fact]
        public async Task BroadcastToSelf_2()
        {
            // NOTE: Register `Non-self` target client at first.
            var mockReceiverOther = new StreamingHubBroadcastTestHubReceiverMock();
            var hubOther = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiverOther);
            await hubOther.RegisterConnectionToGroup();

            var mockReceiver = new StreamingHubBroadcastTestHubReceiverMock();
            var hub = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiver);
            await hub.RegisterConnectionToGroup();

            Assert.False(mockReceiver.HasCalled);
            Assert.False(mockReceiverOther.HasCalled);

            await hub.CallBroadcastToSelfAsync();

            await Task.Delay(10); // NOTE: The receivers may not receive broadcast yet at this point.

            // Target: Self
            Assert.True(mockReceiver.HasCalled);
            Assert.False(mockReceiverOther.HasCalled);
        }

        [Fact]
        public async Task BroadcastToExceptSelf()
        {
            var mockReceiver = new StreamingHubBroadcastTestHubReceiverMock();
            var hub = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiver);
            await hub.RegisterConnectionToGroup();

            var mockReceiverOther = new StreamingHubBroadcastTestHubReceiverMock();
            var hubOther = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiverOther);
            await hubOther.RegisterConnectionToGroup();

            Assert.False(mockReceiver.HasCalled);
            Assert.False(mockReceiverOther.HasCalled);

            await hub.CallBroadcastExceptSelfAsync();

            await Task.Delay(10); // NOTE: The receivers may not receive broadcast yet at this point.

            // Target: Other
            Assert.False(mockReceiver.HasCalled);
            Assert.True(mockReceiverOther.HasCalled);
        }

        [Fact]
        public async Task BroadcastToExceptSelf_2()
        {
            // NOTE: Register `Non-self` target client at first.
            var mockReceiverOther = new StreamingHubBroadcastTestHubReceiverMock();
            var hubOther = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiverOther);
            await hubOther.RegisterConnectionToGroup();

            var mockReceiver = new StreamingHubBroadcastTestHubReceiverMock();
            var hub = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiver);
            await hub.RegisterConnectionToGroup();

            Assert.False(mockReceiver.HasCalled);
            Assert.False(mockReceiverOther.HasCalled);

            await hub.CallBroadcastExceptSelfAsync();

            await Task.Delay(TimeSpan.FromMilliseconds(100)); // NOTE: The receivers may not receive broadcast yet at this point.

            // Target: Other
            Assert.False(mockReceiver.HasCalled);
            Assert.True(mockReceiverOther.HasCalled);
        }

        [Fact]
        public async Task BroadcastToExcept_One_1()
        {
            var mockReceiver = new StreamingHubBroadcastTestHubReceiverMock();
            var hub = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiver);
            var connectionId = await hub.RegisterConnectionToGroup();

            var mockReceiverOther = new StreamingHubBroadcastTestHubReceiverMock();
            var hubOther = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiverOther);
            var connectionIdOther = await hubOther.RegisterConnectionToGroup();

            Assert.False(mockReceiver.HasCalled);
            Assert.False(mockReceiverOther.HasCalled);

            await hub.CallBroadcastExceptAsync(Guid.NewGuid());

            await Task.Delay(10); // NOTE: The receivers may not receive broadcast yet at this point.

            // Target: Self, Other
            Assert.True(mockReceiver.HasCalled);
            Assert.True(mockReceiverOther.HasCalled);
        }

        [Fact]
        public async Task BroadcastToExcept_One_2()
        {
            var mockReceiver = new StreamingHubBroadcastTestHubReceiverMock();
            var hub = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiver);
            var connectionId = await hub.RegisterConnectionToGroup();

            var mockReceiverOther = new StreamingHubBroadcastTestHubReceiverMock();
            var hubOther = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiverOther);
            var connectionIdOther = await hubOther.RegisterConnectionToGroup();

            Assert.False(mockReceiver.HasCalled);
            Assert.False(mockReceiverOther.HasCalled);

            await hub.CallBroadcastExceptAsync(connectionIdOther);

            await Task.Delay(10); // NOTE: The receivers may not receive broadcast yet at this point.

            // Target: Self
            Assert.True(mockReceiver.HasCalled);
            Assert.False(mockReceiverOther.HasCalled);
        }

        [Fact]
        public async Task BroadcastToExcept_One_3()
        {
            var mockReceiver = new StreamingHubBroadcastTestHubReceiverMock();
            var hub = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiver);
            var connectionId = await hub.RegisterConnectionToGroup();

            var mockReceiverOther = new StreamingHubBroadcastTestHubReceiverMock();
            var hubOther = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiverOther);
            var connectionIdOther = await hubOther.RegisterConnectionToGroup();

            Assert.False(mockReceiver.HasCalled);
            Assert.False(mockReceiverOther.HasCalled);

            await hub.CallBroadcastExceptAsync(connectionId);

            await Task.Delay(10); // NOTE: The receivers may not receive broadcast yet at this point.

            // Target: Other
            Assert.False(mockReceiver.HasCalled);
            Assert.True(mockReceiverOther.HasCalled);
        }

        [Fact]
        public async Task BroadcastToExcept_Many_1()
        {
            var mockReceiver = new StreamingHubBroadcastTestHubReceiverMock();
            var hub = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiver);
            var connectionId = await hub.RegisterConnectionToGroup();

            var mockReceiverOther = new StreamingHubBroadcastTestHubReceiverMock();
            var hubOther = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiverOther);
            var connectionIdOther = await hubOther.RegisterConnectionToGroup();

            Assert.False(mockReceiver.HasCalled);
            Assert.False(mockReceiverOther.HasCalled);

            await hub.CallBroadcastExceptManyAsync(new[] { Guid.NewGuid(), Guid.NewGuid() });

            await Task.Delay(10); // NOTE: The receivers may not receive broadcast yet at this point.

            // Target: Self, Other
            Assert.True(mockReceiver.HasCalled);
            Assert.True(mockReceiverOther.HasCalled);
        }

        [Fact]
        public async Task BroadcastToExcept_Many_2()
        {
            var mockReceiver = new StreamingHubBroadcastTestHubReceiverMock();
            var hub = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiver);
            var connectionId = await hub.RegisterConnectionToGroup();

            var mockReceiverOther = new StreamingHubBroadcastTestHubReceiverMock();
            var hubOther = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiverOther);
            var connectionIdOther = await hubOther.RegisterConnectionToGroup();

            Assert.False(mockReceiver.HasCalled);
            Assert.False(mockReceiverOther.HasCalled);

            await hub.CallBroadcastExceptManyAsync(new[] { Guid.NewGuid(), connectionIdOther, Guid.NewGuid() });

            await Task.Delay(10); // NOTE: The receivers may not receive broadcast yet at this point.

            // Target: Self
            Assert.True(mockReceiver.HasCalled);
            Assert.False(mockReceiverOther.HasCalled);
        }

        [Fact]
        public async Task BroadcastToExcept_Many_3()
        {
            var mockReceiver = new StreamingHubBroadcastTestHubReceiverMock();
            var hub = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiver);
            var connectionId = await hub.RegisterConnectionToGroup();

            var mockReceiverOther = new StreamingHubBroadcastTestHubReceiverMock();
            var hubOther = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiverOther);
            var connectionIdOther = await hubOther.RegisterConnectionToGroup();

            Assert.False(mockReceiver.HasCalled);
            Assert.False(mockReceiverOther.HasCalled);

            await hub.CallBroadcastExceptManyAsync(new[] { Guid.NewGuid(), connectionIdOther, Guid.NewGuid(), connectionId, Guid.NewGuid() });

            await Task.Delay(10); // NOTE: The receivers may not receive broadcast yet at this point.

            // Target: None
            Assert.False(mockReceiver.HasCalled);
            Assert.False(mockReceiverOther.HasCalled);
        }


        [Fact]
        public async Task BroadcastTo_One_1()
        {
            var mockReceiver = new StreamingHubBroadcastTestHubReceiverMock();
            var hub = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiver);
            var connectionId = await hub.RegisterConnectionToGroup();

            var mockReceiverOther = new StreamingHubBroadcastTestHubReceiverMock();
            var hubOther = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiverOther);
            var connectionIdOther = await hubOther.RegisterConnectionToGroup();

            Assert.False(mockReceiver.HasCalled);
            Assert.False(mockReceiverOther.HasCalled);

            await hub.CallBroadcastToAsync(Guid.NewGuid());

            await Task.Delay(10); // NOTE: The receivers may not receive broadcast yet at this point.

            // Target: None
            Assert.False(mockReceiver.HasCalled);
            Assert.False(mockReceiverOther.HasCalled);
        }

        [Fact]
        public async Task BroadcastTo_One_2()
        {
            var mockReceiver = new StreamingHubBroadcastTestHubReceiverMock();
            var hub = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiver);
            var connectionId = await hub.RegisterConnectionToGroup();

            var mockReceiverOther = new StreamingHubBroadcastTestHubReceiverMock();
            var hubOther = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiverOther);
            var connectionIdOther = await hubOther.RegisterConnectionToGroup();

            Assert.False(mockReceiver.HasCalled);
            Assert.False(mockReceiverOther.HasCalled);

            await hub.CallBroadcastToAsync(connectionId);

            await Task.Delay(100); // NOTE: The receivers may not receive broadcast yet at this point.

            // Target: Other
            Assert.True(mockReceiver.HasCalled);
            Assert.False(mockReceiverOther.HasCalled);
        }

        [Fact]
        public async Task BroadcastTo_One_3()
        {
            var mockReceiver = new StreamingHubBroadcastTestHubReceiverMock();
            var hub = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiver);
            var connectionId = await hub.RegisterConnectionToGroup();

            var mockReceiverOther = new StreamingHubBroadcastTestHubReceiverMock();
            var hubOther = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiverOther);
            var connectionIdOther = await hubOther.RegisterConnectionToGroup();

            Assert.False(mockReceiver.HasCalled);
            Assert.False(mockReceiverOther.HasCalled);

            await hub.CallBroadcastToAsync(connectionIdOther);

            await Task.Delay(10); // NOTE: The receivers may not receive broadcast yet at this point.

            // Target: Other
            Assert.False(mockReceiver.HasCalled);
            Assert.True(mockReceiverOther.HasCalled);
        }

        [Fact]
        public async Task BroadcastTo_Many_1()
        {
            var mockReceiver = new StreamingHubBroadcastTestHubReceiverMock();
            var hub = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiver);
            var connectionId = await hub.RegisterConnectionToGroup();

            var mockReceiverOther = new StreamingHubBroadcastTestHubReceiverMock();
            var hubOther = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiverOther);
            var connectionIdOther = await hubOther.RegisterConnectionToGroup();

            Assert.False(mockReceiver.HasCalled);
            Assert.False(mockReceiverOther.HasCalled);

            await hub.CallBroadcastToManyAsync(new[] { Guid.NewGuid(), Guid.NewGuid() });

            await Task.Delay(10); // NOTE: The receivers may not receive broadcast yet at this point.

            // Target: None
            Assert.False(mockReceiver.HasCalled);
            Assert.False(mockReceiverOther.HasCalled);
        }

        [Fact]
        public async Task BroadcastTo_Many_2()
        {
            var mockReceiver = new StreamingHubBroadcastTestHubReceiverMock();
            var hub = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiver);
            var connectionId = await hub.RegisterConnectionToGroup();

            var mockReceiverOther = new StreamingHubBroadcastTestHubReceiverMock();
            var hubOther = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiverOther);
            var connectionIdOther = await hubOther.RegisterConnectionToGroup();

            Assert.False(mockReceiver.HasCalled);
            Assert.False(mockReceiverOther.HasCalled);

            await hub.CallBroadcastToManyAsync(new[] { Guid.NewGuid(), connectionId, Guid.NewGuid() });

            await Task.Delay(10); // NOTE: The receivers may not receive broadcast yet at this point.

            // Target: Self
            Assert.True(mockReceiver.HasCalled);
            Assert.False(mockReceiverOther.HasCalled);
        }

        [Fact]
        public async Task BroadcastTo_Many_3()
        {
            var mockReceiver = new StreamingHubBroadcastTestHubReceiverMock();
            var hub = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiver);
            var connectionId = await hub.RegisterConnectionToGroup();

            var mockReceiverOther = new StreamingHubBroadcastTestHubReceiverMock();
            var hubOther = server.CreateStreamingHubClient<IStreamingHubBroadcastTestHub, IStreamingHubBroadcastTestHubReceiver>(mockReceiverOther);
            var connectionIdOther = await hubOther.RegisterConnectionToGroup();

            Assert.False(mockReceiver.HasCalled);
            Assert.False(mockReceiverOther.HasCalled);

            await hub.CallBroadcastToManyAsync(new[] { Guid.NewGuid(), connectionId, Guid.NewGuid(), connectionIdOther });

            await Task.Delay(10); // NOTE: The receivers may not receive broadcast yet at this point.

            // Target: Self, Other
            Assert.True(mockReceiver.HasCalled);
            Assert.True(mockReceiverOther.HasCalled);
        }
    }
}
