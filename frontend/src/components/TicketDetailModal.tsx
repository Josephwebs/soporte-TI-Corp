import { useEffect, useReducer, useCallback } from 'react';
import { 
  Modal, ModalOverlay, ModalContent, ModalHeader, ModalFooter, ModalBody, ModalCloseButton,
  Button, Box, Flex, Text, Badge, Divider, Textarea, Select, Spinner, VStack, useToast, Heading
} from '@chakra-ui/react';
import { apiClient } from '../api/client';
import type { Ticket, Comment } from '../types';

interface Props {
  isOpen: boolean;
  ticketId: string | null;
  onClose: () => void;
  onTicketUpdated: () => void;
}

type State = {
  ticket: Ticket | null;
  comments: Comment[];
  loading: boolean;
  newComment: string;
  submittingComment: boolean;
  statusUpdating: boolean;
};

const initialState: State = {
  ticket: null,
  comments: [],
  loading: false,
  newComment: '',
  submittingComment: false,
  statusUpdating: false,
};

type Action = 
  | { type: 'START_LOAD' }
  | { type: 'LOAD_SUCCESS'; ticket: Ticket; comments: Comment[] }
  | { type: 'LOAD_ERROR' }
  | { type: 'SET_NEW_COMMENT'; payload: string }
  | { type: 'START_SUBMIT_COMMENT' }
  | { type: 'SUBMIT_COMMENT_SUCCESS' }
  | { type: 'SUBMIT_COMMENT_ERROR' }
  | { type: 'START_STATUS_UPDATE' }
  | { type: 'STATUS_UPDATE_END' };

function reducer(state: State, action: Action): State {
  switch (action.type) {
    case 'START_LOAD': return { ...state, loading: true };
    case 'LOAD_SUCCESS': return { ...state, loading: false, ticket: action.ticket, comments: action.comments };
    case 'LOAD_ERROR': return { ...state, loading: false };
    case 'SET_NEW_COMMENT': return { ...state, newComment: action.payload };
    case 'START_SUBMIT_COMMENT': return { ...state, submittingComment: true };
    case 'SUBMIT_COMMENT_SUCCESS': return { ...state, submittingComment: false, newComment: '' };
    case 'SUBMIT_COMMENT_ERROR': return { ...state, submittingComment: false };
    case 'START_STATUS_UPDATE': return { ...state, statusUpdating: true };
    case 'STATUS_UPDATE_END': return { ...state, statusUpdating: false };
    default: return state;
  }
}

export const TicketDetailModal = ({ isOpen, ticketId, onClose, onTicketUpdated }: Props) => {
  const [state, dispatch] = useReducer(reducer, initialState);
  const toast = useToast();

  const loadData = useCallback(async () => {
    if (!ticketId) return;
    try {
      dispatch({ type: 'START_LOAD' });
      const [ticketRes, commentsRes] = await Promise.all([
        apiClient.get<Ticket>(`/tickets/${ticketId}`),
        apiClient.get<Comment[]>(`/tickets/${ticketId}/comments`)
      ]);
      dispatch({ type: 'LOAD_SUCCESS', ticket: ticketRes.data, comments: commentsRes.data });
    } catch (error) {
      console.error("Error loading ticket data", error);
      dispatch({ type: 'LOAD_ERROR' });
    }
  }, [ticketId]);

  useEffect(() => {
    if (!isOpen || !ticketId) return;
    let isMounted = true;
    
    if (isMounted) {
      loadData();
    }
    
    return () => {
      isMounted = false;
    };
  }, [isOpen, ticketId, loadData]);

  const handleAddComment = async () => {
    if (!state.newComment.trim()) return;
    try {
      dispatch({ type: 'START_SUBMIT_COMMENT' });
      await apiClient.post(`/tickets/${ticketId}/comments`, { text: state.newComment });
      dispatch({ type: 'SUBMIT_COMMENT_SUCCESS' });
      loadData();
    } catch (error) {
      console.error("Error adding comment", error);
      dispatch({ type: 'SUBMIT_COMMENT_ERROR' });
    }
  };

  const handleStatusChange = async (newStatus: string) => {
    try {
      dispatch({ type: 'START_STATUS_UPDATE' });
      await apiClient.patch(`/tickets/${ticketId}/status`, { status: newStatus });
      loadData();
      onTicketUpdated();
      toast({ title: 'Estado actualizado', status: 'success', duration: 3000, isClosable: true, position: 'bottom' });
    } catch (error: any) {
      console.error("Error updating status", error);
      const serverMsg = error.response?.data?.error || "No se pudo actualizar el estado.";
      toast({ title: 'Error', description: serverMsg, status: 'error', duration: 5000, isClosable: true, position: 'bottom' });
    } finally {
      dispatch({ type: 'STATUS_UPDATE_END' });
    }
  };

  if (!state.ticket && !state.loading) return null;

  return (
    <Modal isOpen={isOpen} onClose={onClose} size="2xl" scrollBehavior="inside" isCentered>
      <ModalOverlay backdropFilter="blur(4px)" />
      <ModalContent borderRadius="2xl">
        {state.loading || !state.ticket ? (
            <Flex justify="center" p={20}><Spinner size="xl" color="blue.500" thickness="4px" /></Flex>
        ) : (
            <>
                <ModalHeader borderBottomWidth="1px" pb={4}>
                    <Flex justify="space-between" align="center" pr={8}>
                        <Text>Detalle del Ticket</Text>
                        <Select 
                            w="150px" 
                            size="sm" 
                            value={state.ticket.status} 
                            onChange={(e) => handleStatusChange(e.target.value)}
                            isDisabled={state.statusUpdating}
                            borderRadius="md"
                        >
                            <option value="Open">Open</option>
                            <option value="InProgress">InProgress</option>
                            <option value="Resolved">Resolved</option>
                            <option value="Closed">Closed</option>
                        </Select>
                    </Flex>
                </ModalHeader>
                <ModalCloseButton top={4} right={4} />
                
                <ModalBody py={6}>
                    <VStack align="stretch" spacing={6}>
                        <Box>
                            <Heading size="md" mb={3} lineHeight="tall">{state.ticket.title}</Heading>
                            <Flex gap={2} mb={5} align="center">
                                <Badge colorScheme="gray" px={2} py={0.5} rounded="md">{state.ticket.priority}</Badge>
                                <Text fontSize="sm" color="gray.500">
                                    Creado por <Text as="span" fontWeight="600">{state.ticket.createdBy}</Text> el {new Date(state.ticket.createdAt).toLocaleDateString()}
                                </Text>
                            </Flex>
                            <Box p={5} bg="gray.50" rounded="xl" borderWidth="1px" borderColor="transparent" _dark={{ bg: 'whiteAlpha.50', borderColor: 'whiteAlpha.200' }}>
                                <Text whiteSpace="pre-wrap" lineHeight="tall">{state.ticket.description}</Text>
                            </Box>
                        </Box>

                        <Divider />

                        <Box>
                            <Heading size="sm" mb={4} color="gray.700" _dark={{ color: 'gray.200' }}>Comentarios ({state.comments.length})</Heading>
                            <VStack align="stretch" spacing={3} mb={5}>
                                {state.comments.map(c => (
                                    <Box key={c.id} p={4} bg="gray.50" rounded="xl" borderWidth="1px" borderColor="transparent" _dark={{ bg: 'whiteAlpha.50', borderColor: 'whiteAlpha.200' }}>
                                        <Text fontSize="sm" lineHeight="tall">{c.text}</Text>
                                        <Text fontSize="xs" color="gray.500" mt={3} fontWeight="500">
                                            — {c.createdBy} ({new Date(c.createdAt).toLocaleString()})
                                        </Text>
                                    </Box>
                                ))}
                            </VStack>

                            <Flex gap={3} align="flex-start">
                                <Textarea 
                                    placeholder="Escribe un comentario..." 
                                    value={state.newComment}
                                    onChange={(e) => dispatch({ type: 'SET_NEW_COMMENT', payload: e.target.value })}
                                    rows={2}
                                    focusBorderColor="blue.500"
                                    borderRadius="lg"
                                />
                                <Button 
                                    colorScheme="blue" 
                                    onClick={handleAddComment} 
                                    isDisabled={!state.newComment.trim() || state.submittingComment}
                                    isLoading={state.submittingComment}
                                    px={6}
                                    rounded="lg"
                                >
                                    Enviar
                                </Button>
                            </Flex>
                        </Box>
                    </VStack>
                </ModalBody>

                <ModalFooter>
                    <Button variant="ghost" onClick={onClose}>Cerrar</Button>
                </ModalFooter>
            </>
        )}
      </ModalContent>
    </Modal>
  );
};

