import {visit} from 'unist-util-visit';

const plugin = (options) => {
  const transformer = async (ast) => {
    visit(ast, 'heading', (node, index, parent) => {
      if (node.depth === 1 && node.children.length > 0) {
        parent.children.push({
            type: 'containerDirective',
            data: {
                hName: 'AdditionalHeaderMetaRow',
                hProperties: {}
            }
        });
      }
    });
  };
  return transformer;
};

export default plugin;
