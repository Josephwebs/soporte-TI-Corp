import { useState, useEffect } from 'react';
import { Box, Flex, Text, Badge, Grid, GridItem, Button, useDisclosure, Spinner, Center, Heading } from '@chakra-ui/react';
import { m } from 'framer-motion';

import { apiClient } from '../../api/client';
import type { PaginatedResult, Ticket } from '../../types';
import { CreateTicketModal } from '../../components/CreateTicketModal';
import { TicketDetailModal } from '../../components/TicketDetailModal';

const getStatusColorScheme = (status: string) => {
    switch (status) {
        case 'Open': return 'green';
        case 'InProgress': return 'blue';
        case 'Resolved': return 'gray';
        case 'Closed': return 'red';
        default: return 'gray';
    }
};

const getPriorityColorScheme = (priority: string) => {
    switch (priority) {
        case 'Low': return 'gray';
        case 'Medium': return 'blue';
        case 'High': return 'orange';
        case 'Critical': return 'red';
        default: return 'gray';
    }
};

export const TicketList = () => {
    const [data, setData] = useState<PaginatedResult<Ticket>>({ totalCount: 0, items: [] });
    const [loading, setLoading] = useState(true);
    const { isOpen: isCreateOpen, onOpen: onCreateOpen, onClose: onCreateClose } = useDisclosure();
    const { isOpen: isDetailOpen, onOpen: onDetailOpen, onClose: onDetailClose } = useDisclosure();
    const [selectedTicketId, setSelectedTicketId] = useState<string | null>(null);

    useEffect(() => {
        apiClient.get<PaginatedResult<Ticket>>('/tickets')
            .then(response => {
                setData(response.data);
            })
            .catch(error => {
                console.error('Error loading tickets', error);
            })
            .finally(() => {
                setLoading(false);
            });
    }, []);

    const loadTickets = async () => {
        setLoading(true);
        try {
            const response = await apiClient.get<PaginatedResult<Ticket>>('/tickets');
            setData(response.data);
        } catch (error) {
            console.error('Error loading tickets', error);
        } finally {
            setLoading(false);
        }
    };

    if (loading) {
        return (
            <Center py={10}>
                <Spinner size="xl" color="blue.500" />
            </Center>
        );
    }

    return (
        <Box>
            <Flex justify="flex-end" mb={6}>
                <Button colorScheme="blue" onClick={onCreateOpen} rounded="full" px={6}>
                    + Nuevo Ticket
                </Button>
            </Flex>

            <Grid templateColumns={{ base: "1fr", md: "repeat(2, 1fr)", lg: "repeat(3, 1fr)" }} gap={6}>
                {data.items.length === 0 ? (
                    <GridItem colSpan={{ base: 1, md: 2, lg: 3 }}>
                        <Text textAlign="center" color="gray.500" py={10}>
                            No hay tickets activos en este momento.
                        </Text>
                    </GridItem>
                ) : (
                    data.items.map((ticket, index) => (
                        <m.div
                            key={ticket.id}
                            initial={{ opacity: 0, y: 30 }}
                            animate={{ opacity: 1, y: 0 }}
                            transition={{ duration: 0.4, delay: index * 0.1, type: "spring", bounce: 0.4 }}
                            whileHover={{ y: -6, scale: 1.02, transition: { duration: 0.2 } }}
                        >
                            <Box
                                borderWidth="1px" 
                                borderRadius="2xl" 
                                p={6} 
                                cursor="pointer"
                                bg="white"
                                _dark={{ bg: 'whiteAlpha.50', borderColor: 'whiteAlpha.200' }}
                                _hover={{ 
                                    borderColor: "blue.500", 
                                    shadow: "xl",
                                    _dark: { bg: 'whiteAlpha.100' }
                                }}
                                onClick={() => {
                                    setSelectedTicketId(ticket.id);
                                    onDetailOpen();
                                }}
                                h="100%"
                            >
                                <Heading size="sm" mb={3} noOfLines={1} fontWeight="600">{ticket.title}</Heading>
                            <Text color="gray.500" _dark={{ color: 'gray.400' }} fontSize="sm" mb={5} noOfLines={2} lineHeight="tall">
                                {ticket.description}
                            </Text>
                            <Flex justify="space-between" align="center">
                                <Flex gap={2}>
                                    <Badge colorScheme={getStatusColorScheme(ticket.status)} variant="subtle" rounded="md" px={2} py={0.5} textTransform="capitalize">
                                        {ticket.status}
                                    </Badge>
                                    <Badge colorScheme={getPriorityColorScheme(ticket.priority)} variant="outline" rounded="md" px={2} py={0.5} textTransform="capitalize">
                                        {ticket.priority}
                                    </Badge>
                                </Flex>
                                <Text fontSize="xs" color="gray.400" fontWeight="500">
                                    {new Date(ticket.createdAt).toLocaleDateString()}
                                </Text>
                            </Flex>
                            </Box>
                        </m.div>
                    ))
                )}
            </Grid>

            <CreateTicketModal 
                isOpen={isCreateOpen} 
                onClose={onCreateClose} 
                onTicketCreated={loadTickets} 
            />

            <TicketDetailModal
                isOpen={isDetailOpen}
                ticketId={selectedTicketId}
                onClose={onDetailClose}
                onTicketUpdated={loadTickets}
            />
        </Box>
    );
};
