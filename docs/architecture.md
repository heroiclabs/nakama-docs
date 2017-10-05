[System Topology]: /images/system_topology_display.png "System Topology"

# Nakama Architecture 

Before you deploy Nakama for testing or production, it is best to understand the overall architecture of Nakama and some best practices to follow. Nakama has been designed to be incredibly simple to deploy and operationalise to allow developers to focus on building their games and apps. 

Below is a simple topology breakdown of the recommended way to run Nakama and CockroachDB behind a load balancer. 
![System Topology][system_topology_display]

As you can see above, you should run Nakama behind a load balancer in order to distribute users equally across the Nakama nodes and prevent overload of individual nodes. Nakama's unique clustering technology handles the sharing of data across nodes in order for players to communicate and compete with users across the cluster with no delay. You can then setup a cluster of CockroachDB nodes which connect directly to Nakama. CockroachDB itself should only be accessible via the Nakama nodes and never be directly connected to the loadbalancer or users themselves for security purposes. 

!!! warning "Firewall Rules"
    For security purposes, it is essential that you run Nakama behind a firewall to prevent malicious attacks and behaviour. You will want to "hide" or "disallow" Nakama's dashboard which runs on port 7351 in your firewall rules. The main API port is 7350 and should be left exposed for your users to connect.

# Hardware Requirements

Nakama itself should run with 1 CPU and a minimum of 1gb of ram for testing. In production, 3 GB+ is the minimum recommended hardware. As all active connections are stored in-memory for the presence system, memory is often the first point of pressure for Nakama and you should over-provision where possible.

For prototyping and testing, you can run Nakama and CockroachDB on the same instance. However, in production each should run on their own, independent instance for stability and resource availability. The CockroachDB team recommends a bare minimum of 1 CPU and 2 GB of RAM per node. In production, we recommend a minimum of 4 GB and ideally 6 GB+. It is also recommended to run CockroachDB on an SSD rather than an HDD for performance benefits. 

# Deployment Best Practices

Nakama should be run on its own instance and installed onto a fresh environment. We recommend a minimum of 3 nodes in production for failover and tolerance. At the very minimum you should run 2 nodes to guarantee availability for your users.    

For the database, the CockroachDB team recommends running 3 nodes to ensure that the majority of the replicas (2/3) remain available if one node were to fail. It is better to deploy many smaller nodes than a few large ones as the strength in CockroachDB is its data replication across nodes for failover and backup. 


    
    
    
