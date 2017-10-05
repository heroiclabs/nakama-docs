[System Topology]: /images/system_topology_display.png "System Topology"

# Nakama Architecture 

Before you deploy Nakama for testing or production, it is best to understand the overall architecture of Nakama and some best practices to follow. Nakama has been designed to be incredibly simple to deploy and operationalise to allow developers to focus on building their games and apps. 

Below is a simple topology breakdown of the recommended way to run Nakama and CockroachDB behind a loadbalancer. 
![System Topology][system_topology_display]

You should run Nakama behind a load balancer when running a cluster of Nakama nodes in order to distribute users equally across the nodes and prevent overload of individual nodes. Nakama's unique clustering technology handles the sharing of data across nodes in order for players to communicate and compete with those across the cluster with no delay. CockroachDB itself should only be accessible via the Nakama nodes and never be directly connected to the loadbalancer or users themselves. 

!!! warning "Firewall Rules"
    For security purposes, it is essential that you run Nakama behind a firewall to prevent malicious attacks and behaviour. You will want to "hide" or "disallow" Nakama's dashboard which runs on port 7351 in your firewall rules. The main API port is 7350 and should be left exposed for your users to connect.

# Deployment Best Practices

For prototyping and testing, you can run Nakama and CockroachDB on the same instance. However, in production each should run on their own, independent instance for stability and resource availability. The CockroachDB team recommends a bare minimum of 1 CPU and 2 GB of RAM per node. In production, we recommend a minimum of 4 GB and ideally 6 GB+. 

Nakama itself should run with 1 CPU and a minimum of 1gb of ram for testing. In production, 3 GB+ is the minimum recommended hardware. As all active connections are stored in-memory for the presence system, memory is often the first point of pressure for Nakama and you should over-provision where possible.




    
    
    
