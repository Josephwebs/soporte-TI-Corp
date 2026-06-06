import { 
  Modal, ModalOverlay, ModalContent, ModalHeader, ModalFooter, ModalBody, ModalCloseButton,
  Button, FormControl, FormLabel, Input, Textarea, Select, FormErrorMessage, VStack
} from '@chakra-ui/react';
import { useForm } from 'react-hook-form';
import { m, AnimatePresence } from 'framer-motion';
import * as yup from 'yup';
import { yupResolver } from '@hookform/resolvers/yup';
import { apiClient } from '../api/client';

const schema = yup.object().shape({
  title: yup.string().required('El título es requerido').min(5, 'Mínimo 5 caracteres').max(120, 'Máximo 120 caracteres'),
  description: yup.string().required('La descripción es requerida').min(10, 'Mínimo 10 caracteres').max(2000, 'Máximo 2000 caracteres'),
  priority: yup.string().oneOf(['Low', 'Medium', 'High', 'Critical']).required('La prioridad es requerida')
});

type FormData = yup.InferType<typeof schema>;

interface Props {
  isOpen: boolean;
  onClose: () => void;
  onTicketCreated: () => void;
}

export const CreateTicketModal = ({ isOpen, onClose, onTicketCreated }: Props) => {
  const { register, handleSubmit, formState: { errors, isSubmitting }, reset } = useForm<FormData>({
    resolver: yupResolver(schema) as any,
    defaultValues: { title: '', description: '', priority: 'Low' }
  });

  const onSubmit = async (data: FormData) => {
    try {
      await apiClient.post('/tickets', data);
      reset();
      onTicketCreated();
      onClose();
    } catch (error) {
      console.error("Error creating ticket", error);
      alert("Error al crear el ticket.");
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} size="xl" isCentered>
      <ModalOverlay backdropFilter="blur(4px)" />
      <ModalContent borderRadius="2xl">
        <ModalHeader>Crear Nuevo Ticket</ModalHeader>
        <ModalCloseButton />
        
        <form onSubmit={handleSubmit(onSubmit)}>
          <ModalBody>
            <VStack spacing={5}>
              <FormControl isInvalid={!!errors.title}>
                <FormLabel>Título del Ticket</FormLabel>
                <Input {...register('title')} placeholder="Ej. Caída de servidor principal" focusBorderColor="blue.500" />
                <AnimatePresence mode="popLayout">
                  {errors.title && (
                    <m.div
                      key="title-error"
                      initial={{ opacity: 0, height: 0, y: -10 }}
                      animate={{ opacity: 1, height: 'auto', y: 0 }}
                      exit={{ opacity: 0, height: 0, y: -10 }}
                    >
                      <FormErrorMessage mt={2}>
                        {errors.title?.message}
                      </FormErrorMessage>
                    </m.div>
                  )}
                </AnimatePresence>
              </FormControl>

              <FormControl isInvalid={!!errors.priority}>
                <FormLabel>Prioridad</FormLabel>
                <Select {...register('priority')} focusBorderColor="blue.500">
                  <option value="Low">Low</option>
                  <option value="Medium">Medium</option>
                  <option value="High">High</option>
                  <option value="Critical">Critical</option>
                </Select>
                <AnimatePresence mode="popLayout">
                  {errors.priority && (
                    <m.div
                      key="priority-error"
                      initial={{ opacity: 0, height: 0, y: -10 }}
                      animate={{ opacity: 1, height: 'auto', y: 0 }}
                      exit={{ opacity: 0, height: 0, y: -10 }}
                    >
                      <FormErrorMessage mt={2}>
                        {errors.priority?.message}
                      </FormErrorMessage>
                    </m.div>
                  )}
                </AnimatePresence>
              </FormControl>

              <FormControl isInvalid={!!errors.description}>
                <FormLabel>Descripción detallada</FormLabel>
                <Textarea {...register('description')} rows={5} placeholder="Describe el problema con el mayor detalle posible..." focusBorderColor="blue.500" />
                <AnimatePresence mode="popLayout">
                  {errors.description && (
                    <m.div
                      key="desc-error"
                      initial={{ opacity: 0, height: 0, y: -10 }}
                      animate={{ opacity: 1, height: 'auto', y: 0 }}
                      exit={{ opacity: 0, height: 0, y: -10 }}
                    >
                      <FormErrorMessage mt={2}>
                        {errors.description?.message}
                      </FormErrorMessage>
                    </m.div>
                  )}
                </AnimatePresence>
              </FormControl>
            </VStack>
          </ModalBody>

          <ModalFooter borderTopWidth="1px" mt={6} pt={4}>
            <Button variant="ghost" mr={3} onClick={onClose} rounded="full">Cancelar</Button>
            <Button colorScheme="blue" type="submit" isLoading={isSubmitting} rounded="full" px={8}>
              Crear Ticket
            </Button>
          </ModalFooter>
        </form>
      </ModalContent>
    </Modal>
  );
};
