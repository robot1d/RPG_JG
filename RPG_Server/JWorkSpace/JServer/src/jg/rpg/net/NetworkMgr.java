package jg.rpg.net;

import java.io.UnsupportedEncodingException;
import java.security.cert.CertificateException;

import javax.net.ssl.SSLException;

import org.apache.log4j.Logger;

import io.netty.bootstrap.ServerBootstrap;
import io.netty.channel.ChannelFuture;
import io.netty.channel.ChannelInitializer;
import io.netty.channel.ChannelOption;
import io.netty.channel.ChannelPipeline;
import io.netty.channel.EventLoopGroup;
import io.netty.channel.nio.NioEventLoopGroup;
import io.netty.channel.socket.SocketChannel;
import io.netty.channel.socket.nio.NioServerSocketChannel;
import io.netty.handler.codec.LineBasedFrameDecoder;
import io.netty.handler.codec.string.StringDecoder;
import io.netty.handler.codec.string.StringEncoder;
import io.netty.handler.logging.LogLevel;
import io.netty.handler.logging.LoggingHandler;
import io.netty.handler.ssl.SslContext;
import io.netty.handler.ssl.SslContextBuilder;
import io.netty.handler.ssl.util.SelfSignedCertificate;
import io.netty.handler.stream.ChunkedWriteHandler;
import io.netty.util.CharsetUtil;
import jg.rpg.entity.NetEntityInfo;
import jg.rpg.net.handlers.DataEnsureHandler;

public class NetworkMgr{

	private static NetworkMgr inst;
	private ChannelFuture channelFuture;
	static final boolean SSL = System.getProperty("ssl") != null;
	private Logger logger = Logger.getLogger(getClass());
	private NetworkMgr(){}
	
	public static NetworkMgr getInstance(){
		if(inst == null){
			synchronized (NetworkMgr.class) {
				inst = new NetworkMgr();
			}
		}
		return inst;
	}
	
	public void init(NetEntityInfo info) throws CertificateException, SSLException, InterruptedException, UnsupportedEncodingException {
		final SslContext sslCtx;
        SelfSignedCertificate ssc = null;
        if (SSL) {
				ssc = new SelfSignedCertificate();
				sslCtx = SslContextBuilder.forServer(ssc.certificate(), ssc.privateKey()).build();
        } else {
            sslCtx = null;
        }
        EventLoopGroup bossGroup = new NioEventLoopGroup(1);
        EventLoopGroup workerGroup = new NioEventLoopGroup();
        try {
            ServerBootstrap b = new ServerBootstrap();
            b.group(bossGroup, workerGroup)
             .channel(NioServerSocketChannel.class)
             .option(ChannelOption.SO_BACKLOG, 100)
             .handler(new LoggingHandler(LogLevel.INFO))
             .childHandler(new RPGChannelInitializer());
           /*  .childHandler(new ChannelInitializer<SocketChannel>() {
                 @Override
                 public void initChannel(SocketChannel ch) throws Exception {
                     ChannelPipeline p = ch.pipeline();
                     if (sslCtx != null) {
                         p.addLast(sslCtx.newHandler(ch.alloc()));
                     }
                     logger.debug("register handler");
                     p.addLast(
                             new StringEncoder(CharsetUtil.UTF_8),
                             new LineBasedFrameDecoder(8192),
                             new StringDecoder(CharsetUtil.UTF_8),
                             new ChunkedWriteHandler(),
                    		 new LineBasedFrameDecoder(1024*1024*2),
                             new DataEnsureHandler());
                 }
             });*/
           // channelFuture =  b.bind("127.0.0.1", 12345).sync();
            channelFuture = b.bind(12345).sync();
            channelFuture.channel().closeFuture().sync();
        } finally {
            bossGroup.shutdownGracefully();
            workerGroup.shutdownGracefully();
        }
	}	
}
