import { ChakraProvider, extendTheme, Box, Heading, Flex, Text } from '@chakra-ui/react';
import { TicketList } from './features/tickets/TicketList';
import { LazyMotion, domAnimation, MotionConfig, useReducedMotion } from 'framer-motion';

const theme = extendTheme({
  config: {
    initialColorMode: 'system',
    useSystemColorMode: true,
  },
  fonts: {
    heading: `'Inter', sans-serif`,
    body: `'Inter', sans-serif`,
  }
});

function App() {
  useReducedMotion();
  return (
    <ChakraProvider theme={theme}>
      <MotionConfig reducedMotion="user">
        <LazyMotion features={domAnimation}>
          <Box minH="100vh" bg="var(--chakra-colors-chakra-body-bg)">
            <Box as="nav" borderBottom="1px" borderColor="inherit" p={4}>
              <Flex maxW="1200px" mx="auto" align="center">
                <Box w={8} h={8} bg="blue.500" borderRadius="md" mr={3} />
                <Heading size="md" fontWeight="bold">Soporte TI Corp</Heading>
              </Flex>
            </Box>
            <Box as="main" p={8}>
              <Box maxW="1200px" mx="auto">
                <Heading size="xl" mb={2}>Incidencias</Heading>
                <Text color="gray.500" mb={8}>Gestiona y monitorea los tickets de soporte activos.</Text>
                <TicketList />
              </Box>
            </Box>
          </Box>
        </LazyMotion>
      </MotionConfig>
    </ChakraProvider>
  );
}

export default App;
